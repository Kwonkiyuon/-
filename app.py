from flask import Flask, render_template_string, request
import sqlite3
import os
import requests

app = Flask(__name__)

DB_URL = "https://github.com/Kwonkiyuon/-/releases/download/v1.1/default.db"
DB_PATH = "/tmp/default.db"

# DB가 없으면 GitHub Releases에서 다운로드
if not os.path.exists(DB_PATH):
    print("DB 파일이 없습니다. GitHub에서 다운로드 중...")
    response = requests.get(DB_URL)
    with open(DB_PATH, "wb") as f:
        f.write(response.content)
    print("DB 다운로드 완료!")

HTML_TEMPLATE = """
<!DOCTYPE html>
<html>
<head>
    <title>다차종 생산량 뷰어</title>
    <style>
        body { font-family: Arial, sans-serif; padding: 20px; }
        table { border-collapse: collapse; width: 100%; margin-top: 20px; }
        th, td { border: 1px solid #ccc; padding: 8px; text-align: center; }
        th { background-color: #f2f2f2; }
        input[type=text], input[type=date] { padding: 5px; margin-right: 10px; }
        .error { color: red; margin-top: 10px; }
        .logo {
            position: absolute;
            top: 10px;
            right: 10px;
        }
        .logo img {
            height: 75px; /* 1.5배 키움 */
        }
    </style>
</head>
<body>
    <div class="logo">
        <img src="/static/현대인 로고.png" alt="현대인 로고">
    </div>
    <h1>다차종 일일 생산량 조회</h1>
    <form method='get'>
        시작 날짜: <input type='date' name='start_date' value='{{ request.args.get("start_date", "") }}'>
        종료 날짜: <input type='date' name='end_date' value='{{ request.args.get("end_date", "") }}'>
        ALC 코드: <input type='text' name='alc' value='{{ request.args.get("alc", "") }}'>
        <button type='submit'>조회</button>
    </form>
    {% if error %}
        <p class='error'>{{ error }}</p>
    {% endif %}
    {% if data and data|length > 0 %}
        <h2>생산량 조회 결과</h2>
        <table>
            <tr>
                {% for col in data[0].keys() %}
                <th>{{ col }}</th>
                {% endfor %}
            </tr>
            {% for row in data %}
            <tr>
                {% for col in row.values() %}
                <td>{{ col }}</td>
                {% endfor %}
            </tr>
            {% endfor %}
        </table>
    {% endif %}
</body>
</html>
"""

@app.route('/')
def index():
    start_date = request.args.get('start_date', '')
    end_date = request.args.get('end_date', '')
    alc_filter = request.args.get('alc', '').upper()
    error = ''
    data = []

    if not alc_filter:
        error = 'ALC 코드를 입력해 주세요.'
    else:
        try:
            conn = sqlite3.connect(DB_PATH)
            cursor = conn.cursor()

            # 14일 초과 조회 제한
            if start_date and end_date:
                import datetime
                start_dt = datetime.datetime.strptime(start_date, '%Y-%m-%d')
                end_dt = datetime.datetime.strptime(end_date, '%Y-%m-%d')
                if (end_dt - start_dt).days > 14:
                    error = '조회 기간은 최대 14일까지 가능합니다.'
                    return render_template_string(HTML_TEMPLATE, data=[], request=request, error=error)
            else:
                error = '시작 날짜와 종료 날짜를 모두 입력해주세요.'
                return render_template_string(HTML_TEMPLATE, data=[], request=request, error=error)

            query = """
            SELECT
                [part order done date] as 날짜,
                UPPER(ALC) as ALC,
                UPPER(부품명) as 부품명,
                UPPER(부품_번호) as 부품번호,
                COUNT(*) as 생산수량
            FROM 생산량
            WHERE
                UPPER(ALC) LIKE ?
                AND [part order done date] BETWEEN ? AND ?
            GROUP BY 날짜, ALC, 부품명, 부품번호
            ORDER BY 날짜, 부품명
            """
            params = (f"%{alc_filter}%", start_date, end_date)
            cursor.execute(query, params)
            columns = [desc[0] for desc in cursor.description]
            data = [dict(zip(columns, row)) for row in cursor.fetchall()]

            conn.close()
        except Exception as e:
            error = f'DB 조회 오류: {str(e)}'

    return render_template_string(HTML_TEMPLATE, data=data, request=request, error=error)

if __name__ == '__main__':
    app.run(debug=True)
