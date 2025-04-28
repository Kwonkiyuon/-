from flask import Flask, render_template_string, request
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
<html lang="ko">
<head>
    <meta charset="UTF-8">
    <title>다차종 일일 생산량 조회</title>
    <style>
        body { font-family: Arial, sans-serif; padding: 20px; }
        .top-right { position: absolute; top: 10px; right: 10px; }
        table { border-collapse: collapse; width: 100%; margin-top: 20px; }
        th, td { border: 1px solid #ccc; padding: 8px; text-align: center; }
        th { background-color: #f2f2f2; }
        input[type="date"], input[type="text"] { padding: 5px; margin-right: 10px; }
        button { padding: 5px 10px; }
        .error { color: red; margin-top: 20px; font-weight: bold; }
    </style>
</head>
<body>
    <div class="top-right">
        <img src="/static/현대인 마크.png" alt="현대인 로고" style="height: 90px;">
    </div>

    <h1>다차종 일일 생산량 조회</h1>
    <form method="get">
        시작 날짜: <input type="date" name="start_date" value="{{ request.args.get('start_date', '') }}">
        종료 날짜: <input type="date" name="end_date" value="{{ request.args.get('end_date', '') }}">
        ALC 코드: <input type="text" name="alc" value="{{ request.args.get('alc', '') }}">
        <button type="submit">조회</button>
    </form>

    {% if error %}
        <div class="error">{{ error }}</div>
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

    # 입력값 검증
    if request.args:
        if not (start_date and end_date and alc):
            error = "⚠️ 시작 날짜, 종료 날짜, ALC 코드를 모두 입력해야 합니다."
        else:
            try:
                conn = sqlite3.connect(DB_PATH)
                query = """
                    SELECT 
                        [part order done date] AS 날짜,
                        UPPER(ALC) AS ALC,
                        UPPER(부품명) AS 부품명,
                        UPPER([부품 번호]) AS 부품번호,
                        COUNT(*) AS 생산수량
                    FROM 생산량
                    WHERE 
                        UPPER(ALC) LIKE ?
                        AND [part order done date] BETWEEN ? AND ?
                    GROUP BY 날짜, ALC, 부품명, 부품번호
                    ORDER BY 날짜, 부품명
                """
                df = pd.read_sql_query(query, conn, params=[f"%{alc}%", start_date, end_date])
                conn.close()
                data = df
            except Exception as e:
                error = f"DB 조회 오류: {str(e)}"

    return render_template_string(HTML_TEMPLATE, data=data, request=request, error=error)

if __name__ == '__main__':
    app.run(debug=True)
