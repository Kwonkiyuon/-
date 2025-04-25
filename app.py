from flask import Flask, render_template_string, request
import pandas as pd
import os
import requests

app = Flask(__name__)

CSV_URL = "https://github.com/Kwonkiyuon/-/releases/download/v1.0/통합생산량.csv"
CSV_PATH = "통합생산량.csv"

# CSV가 없으면 GitHub에서 다운로드
if not os.path.exists(CSV_PATH):
    print("CSV 파일이 없습니다. GitHub에서 다운로드 중...")
    response = requests.get(CSV_URL)
    with open(CSV_PATH, "wb") as f:
        f.write(response.content)
    print("CSV 다운로드 완료!")

HTML_TEMPLATE = """
<!DOCTYPE html>
<html>
<head>
    <title>다차종 생산량 뷰어</title>
    <style>
        body { font-family: Arial, sans-serif; padding: 20px; }
        table { border-collapse: collapse; width: 100%; margin-top: 20px; }
        th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; }
        input[type=text], input[type=date] { padding: 5px; margin-right: 10px; }
        .error { color: red; margin-top: 10px; }
    </style>
</head>
<body>
    <h1>다차종 일일 생산량 조회</h1>
    <form method='get'>
        날짜 (Done Date): <input type='date' name='date' value='{{ request.args.get("date", "") }}'>
        부품 번호: <input type='text' name='part_no' value='{{ request.args.get("part_no", "") }}'>
        ALC 코드: <input type='text' name='alc' value='{{ request.args.get("alc", "") }}'>
        <button type='submit'>조회</button>
    </form>
    {% if error %}
        <p class='error'>{{ error }}</p>
    {% endif %}
    {% if data.shape[0] > 0 %}
        <p><strong>총 생산 수량: {{ data.shape[0] }} EA</strong></p>
        <table>
            <tr>{% for col in data.columns %}<th>{{ col }}</th>{% endfor %}</tr>
            {% for _, row in data.iterrows() %}
            <tr>{% for col in data.columns %}<td>{{ row[col] }}</td>{% endfor %}</tr>
            {% endfor %}
        </table>
    {% endif %}
</body>
</html>
"""

@app.route('/')
def index():
    df = pd.read_csv(CSV_PATH, encoding='utf-8')
    
    date_filter = request.args.get('date', '')
    part_no_filter = request.args.get('part_no', '').upper()
    alc_filter = request.args.get('alc', '').upper()
    error = ''

    if not (date_filter or part_no_filter or alc_filter):
        df = df.iloc[0:0]
        error = '조회하려면 날짜나 ALC 코드를 입력해주세요.'
    else:
        if alc_filter:
            df = df[df['ALC'].astype(str).str.upper().str.contains(alc_filter)]
        if part_no_filter:
            df = df[df['부품 번호'].astype(str).str.upper().str.contains(part_no_filter)]
        if date_filter:
            df = df[df['part order done date'].astype(str).str.startswith(date_filter)]

    return render_template_string(HTML_TEMPLATE, data=df, request=request, error=error)

if __name__ == '__main__':
    app.run(debug=True)
