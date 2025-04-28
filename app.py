from flask import Flask, render_template_string, request
import pandas as pd
import sqlite3
import os
import requests
from datetime import datetime

app = Flask(__name__)

DB_URL = "https://github.com/Kwonkiyuon/-/releases/download/v1.1/default.db"
DB_PATH = "/tmp/default.db"

# DB 파일이 없으면 다운로드
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
        .header { display: flex; justify-content: space-between; align-items: center; }
        .logo { height: 50px; }
        table { border-collapse: collapse; width: 100%; margin-top: 20px; }
        th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; }
        input[type=text], input[type=date] { padding: 5px; margin-right: 10px; }
        .error { color: red; margin-top: 10px; }
    </style>
</head>
<body>
    <div class="header">
        <h1>다차종 일일 생산량 조회</h1>
        <img src="/static/현대인 마크.png" style="position: absolute; top: 10px; right: 10px; height: 50px;">
    </div>
    <form method='get'>
        시작 날짜: <input type='date' name='start_date' value='{{ request.args.get("start_date", "") }}'>
        종료 날짜: <input type='date' name='end_date' value='{{ request.args.get("end_date", "") }}'>
        ALC 코드: <input type='text' name='alc' value='{{ request.args.get("alc", "") }}'>
        <button type='submit'>조회</button>
    </form>
    {% if error %}
        <p class='error'>{{ error }}</p>
    {% endif %}
    {% if results %}
        <h2>생산량 조회 결과</h2>
        <ul>
            {% for item in results %}
                <li>{{ item }}</li>
            {% endfor %}
        </ul>
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
    results = []

    if not (start_date and end_date and alc_filter):
        error = '조회하려면 시작일, 종료일, ALC 코드를 모두 입력해주세요.'
    else:
        try:
            start_dt = datetime.strptime(start_date, "%Y-%m-%d")
            end_dt = datetime.strptime(end_date, "%Y-%m-%d")
            
            if (end_dt - start_dt).days > 14:
                error = '조회 기간은 최대 14일까지만 가능합니다.'
            else:
                conn = sqlite3.connect(DB_PATH)
                query = """
                    SELECT [part order done date], ALC, [부품명], [부품 번호]
                    FROM 생산량
                    WHERE [part order done date] BETWEEN ? AND ?
                    AND UPPER(ALC) LIKE ?
                """
                params = (start_date, end_date, f"%{alc_filter}%")
                df = pd.read_sql_query(query, conn, params=params)
                conn.close()

                if df.empty:
                    error = '해당 조건에 맞는 데이터가 없습니다.'
                else:
                    grouped = df.groupby('part order done date')
                    for date, group in grouped:
                        if group['ALC'].nunique() > 1 or group['부품 번호'].nunique() > 1:
                            for _, row in group.iterrows():
                                results.append(f"{date} - {row['ALC']} - {row['부품명']} - {row['부품 번호']}")
                        else:
                            results.append(f"{date} - 생산량 {len(group)}개")

        except Exception as e:
            error = f"오류 발생: {str(e)}"

    return render_template_string(HTML_TEMPLATE, results=results, request=request, error=error)

if __name__ == '__main__':
    app.run(debug=True)
