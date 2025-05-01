from flask import Flask, render_template_string, request, jsonify
import sqlite3
import os
import requests
import pandas as pd

app = Flask(__name__)

DB_URL = "https://github.com/Kwonkiyuon/-/releases/download/v1.1/default.db"
DB_PATH = "/tmp/default.db"

# DB 파일이 없으면 GitHub에서 다운로드
if not os.path.exists(DB_PATH):
    print("DB 파일이 없습니다. GitHub에서 다운로드 중...")
    response = requests.get(DB_URL)
    with open(DB_PATH, "wb") as f:
        f.write(response.content)
    print("DB 다운로드 완료!")

HTML_TEMPLATE = """
<!DOCTYPE html>
<html lang=\"ko\">
<head>
    <meta charset=\"UTF-8\">
    <meta http-equiv=\"refresh\" content=\"300\"> <!-- 5분마다 새로고침 -->
    <title>다차종 일일 생산량 조회</title>
    <script src=\"https://code.jquery.com/jquery-3.6.0.min.js\"></script>
    <script src=\"https://code.jquery.com/ui/1.13.1/jquery-ui.min.js\"></script>
    <link rel=\"stylesheet\" href=\"https://code.jquery.com/ui/1.13.1/themes/base/jquery-ui.css\">
    <style>
        body { font-family: Arial, sans-serif; padding: 20px; }
        .top-right { position: absolute; top: 10px; right: 10px; }
        table { border-collapse: collapse; width: 100%; margin-top: 20px; }
        th, td { border: 1px solid #ccc; padding: 8px; text-align: center; }
        th { background-color: #f2f2f2; }
        input[type=\"date\"], input[type=\"text\"], select { padding: 5px; margin-right: 10px; }
        button { padding: 5px 10px; }
        .error { color: red; margin-top: 20px; font-weight: bold; }
        .notice { margin-top: 20px; color: red; font-size: 20px; font-weight: bold; }
    </style>
    <script>
        $(function() {
            $("#part_name").autocomplete({
                source: function(request, response) {
                    $.getJSON("/autocomplete_part_name", { term: request.term }, response);
                },
                minLength: 2
            });
            $("#alc").autocomplete({
                source: function(request, response) {
                    $.getJSON("/autocomplete_alc", { term: request.term }, response);
                },
                minLength: 1
            });
        });
    </script>
</head>
<body>
    <div class=\"top-right\">
        <img src=\"/static/현대인 마크.png\" alt=\"현대인 로고\" style=\"height: 90px;\">
    </div>

    <h1>다차종 일일 생산량 조회</h1>
    <form method=\"get\">
        시작 날짜: <input type=\"date\" name=\"start_date\" value=\"{{ request.args.get('start_date', '') }}\">
        종료 날짜: <input type=\"date\" name=\"end_date\" value=\"{{ request.args.get('end_date', '') }}\">
        차종: <select name=\"model\">
            <option value=\"\">선택 안함</option>
            {% for m in models %}
                <option value=\"{{ m }}\" {% if m == request.args.get('model', '') %}selected{% endif %}>{{ m }}</option>
            {% endfor %}
        </select>
        부품명 : <input type=\"text\" id=\"part_name\" name=\"part_name\" value=\"{{ request.args.get('part_name', '') }}\">
        ALC 코드: <input type=\"text\" id=\"alc\" name=\"alc\" value=\"{{ request.args.get('alc', '') }}\">
        <button type=\"submit\">조회</button>
    </form>

    <div class=\"notice\">
        시작 날짜와 종료 날짜는 필수!<br>
        부품명, ALC 코드 또는 차종을 입력해주세요.<br>
        부품명 입력 시 부품의 전체 ALC코드와 생산량을 볼 수 있고,<br>
        ALC 코드를 입력 시 단일 부품만 알 수 있습니다.
    </div>

    {% if error %}
        <div class=\"error\">{{ error }}</div>
    {% endif %}

    {% if data is not none and not data.empty %}
        <h2>생산량 조회 결과</h2>
        <table>
            <thead>
                <tr>
                    {% for col in data.columns %}
                        <th>{{ col }}</th>
                    {% endfor %}
                </tr>
            </thead>
            <tbody>
                {% for row in data.values %}
                    <tr>
                        {% for item in row %}
                            <td>{{ item }}</td>
                        {% endfor %}
                    </tr>
                {% endfor %}
            </tbody>
        </table>
    {% endif %}
</body>
</html>
"""

@app.route('/')
def index():
    error = ''
    data = None

    start_date = request.args.get('start_date', '')
    end_date = request.args.get('end_date', '')
    alc = request.args.get('alc', '').upper()
    part_name = request.args.get('part_name', '').upper()
    model = request.args.get('model', '')

    models = []
    conn = sqlite3.connect(DB_PATH)
    cur = conn.cursor()
    cur.execute("SELECT DISTINCT 차종 FROM 생산량 ORDER BY 차종")
    models = [row[0] for row in cur.fetchall() if row[0]]
    conn.close()

    if request.args:
        if not (start_date and end_date):
            error = "⚠️ 시작 날짜와 종료 날짜는 필수입니다."
        elif not (part_name or alc or model):
            error = "⚠️ 부품명, ALC 코드 또는 차종을 입력해주세요."
        else:
            try:
                conn = sqlite3.connect(DB_PATH)
                query = """
                    SELECT 
                        [part order done date] AS 날짜,
                        차종,
                        UPPER(ALC) AS ALC,
                        UPPER(부품명) AS 부품명,
                        UPPER([부품 번호]) AS 부품번호,
                        COUNT(*) AS 생산수량
                    FROM 생산량
                    WHERE 
                        [part order done date] BETWEEN ? AND ?
                """
                params = [start_date, end_date]

                if model:
                    query += " AND 차종 = ?"
                    params.append(model)
                if alc:
                    query += " AND UPPER(ALC) LIKE ?"
                    params.append(f"%{alc}%")
                if part_name:
                    query += " AND UPPER(부품명) LIKE ?"
                    params.append(f"%{part_name}%")

                query += " GROUP BY 날짜, 차종, ALC, 부품명, 부품번호 ORDER BY 날짜, 부품명"

                df = pd.read_sql_query(query, conn, params=params)
                conn.close()
                data = df
            except Exception as e:
                error = f"DB 조회 오류: {str(e)}"

    return render_template_string(HTML_TEMPLATE, data=data, request=request, error=error, models=models)

@app.route('/autocomplete_part_name')
def autocomplete_part_name():
    term = request.args.get("term", "").upper()
    conn = sqlite3.connect(DB_PATH)
    cur = conn.cursor()
    cur.execute("SELECT DISTINCT 부품명 FROM 생산량 WHERE UPPER(부품명) LIKE ? LIMIT 10", (f"%{term}%",))
    results = [row[0] for row in cur.fetchall()]
    conn.close()
    return jsonify(results)

@app.route('/autocomplete_alc')
def autocomplete_alc():
    term = request.args.get("term", "").upper()
    conn = sqlite3.connect(DB_PATH)
    cur = conn.cursor()
    cur.execute("SELECT DISTINCT ALC FROM 생산량 WHERE UPPER(ALC) LIKE ? LIMIT 10", (f"%{term}%",))
    results = [row[0] for row in cur.fetchall()]
    conn.close()
    return jsonify(results)

if __name__ == '__main__':
    app.run(debug=True)
