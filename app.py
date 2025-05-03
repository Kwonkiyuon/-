import os
import requests
import pandas as pd
from flask import Flask, render_template_string, request, jsonify

# Flask 앱 생성
app = Flask(__name__)

# CSV 파일 경로 및 자동 다운로드
CSV_URL = "https://github.com/Kwonkiyuon/통합생산/releases/download/v1.1/default.csv"
CSV_PATH = "/tmp/default.csv"

if not os.path.exists(CSV_PATH):
    r = requests.get(CSV_URL)
    with open(CSV_PATH, 'wb') as f:
        f.write(r.content)

HTML_TEMPLATE = """
<!DOCTYPE html>
<html lang=\"ko\">
<head>
    <meta charset=\"UTF-8\">
    <meta http-equiv=\"refresh\" content=\"300\">
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
        input[type=date], input[type=text], select { padding: 5px; margin-right: 10px; }
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
                minLength: 1,
                select: function(event, ui) {
                    $('#part_name').val(ui.item.value);
                    $.getJSON("/related_alc", { part_name: ui.item.value }, function(data) {
                        var alcSelect = $('#alc');
                        alcSelect.empty();
                        alcSelect.append($('<option>', { value: '', text: '선택 안함' }));
                        $.each(data, function(i, item) {
                            alcSelect.append($('<option>', { value: item, text: item }));
                        });
                    });
                    return false;
                }
            });

            $("#model").autocomplete({
                source: function(request, response) {
                    $.getJSON("/autocomplete_model", { term: request.term }, response);
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
        차종: <input type=\"text\" id=\"model\" name=\"model\" value=\"{{ request.args.get('model', '') }}\">
        부품명 : <input type=\"text\" id=\"part_name\" name=\"part_name\" value=\"{{ request.args.get('part_name', '') }}\">
        ALC 코드: <select id=\"alc\" name=\"alc\">
            <option value=\"\">선택 안함</option>
            {% for a in alcs %}
                <option value=\"{{ a }}\" {% if a == request.args.get('alc', '') %}selected{% endif %}>{{ a }}</option>
            {% endfor %}
        </select>
        <button type=\"submit\">조회</button>
    </form>

    <div class=\"notice\">
        시작 날짜와 종료 날짜는 필수!<br>
        부품명, ALC 코드 또는 차종을 입력해주세요.<br>
        부품명 입력 시 부품의 전체 ALC코드와 생산량을 볼 수 있고,<br>
        ALC 코드를 입력 시 단일 부품만 알 수 있습니다.<br>
        매일 오전 9시에 전날 생산량을 업데이트합니다. 참고바랍니다.
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
    model = request.args.get('model', '').upper()

    df = pd.read_csv(CSV_PATH, encoding='cp949')
    df['part order done date'] = pd.to_datetime(df['part order done date'], errors='coerce')
    df['ALC'] = df['ALC'].astype(str).str.upper()
    df['부품명'] = df['부품명'].astype(str).str.upper()
    df['차종'] = df['차종'].astype(str).str.upper()
    df['부품 번호'] = df['부품 번호'].astype(str).str.upper()

    alcs = sorted(df['ALC'].dropna().unique())

    if request.args:
        if not (start_date and end_date):
            error = "⚠️ 시작 날짜와 종료 날짜는 필수입니다."
        elif not (part_name or alc or model):
            error = "⚠️ 부품명, ALC 코드 또는 차종을 입력해주세요."
        else:
            mask = (df['part order done date'] >= start_date) & (df['part order done date'] <= end_date)
            if model:
                mask &= df['차종'].str.contains(model)
            if part_name:
                mask &= df['부품명'].str.contains(part_name)
            if alc:
                mask &= df['ALC'].str.contains(alc)
            filtered = df[mask]
            if not filtered.empty:
                data = filtered.groupby(['part order done date', '차종', 'ALC', '부품명', '부품 번호']).size().reset_index(name='생산수량')
            else:
                data = pd.DataFrame()

    return render_template_string(HTML_TEMPLATE, data=data, request=request, error=error, alcs=alcs)

@app.route('/autocomplete_part_name')
def autocomplete_part_name():
    term = request.args.get("term", "").upper()
    df = pd.read_csv(CSV_PATH, encoding='cp949')
    df['부품명'] = df['부품명'].astype(str).str.upper()
    results = df[df['부품명'].str.contains(term, na=False)]['부품명'].dropna().unique().tolist()[:10]
    return jsonify(results)

@app.route('/autocomplete_model')
def autocomplete_model():
    term = request.args.get("term", "").upper()
    df = pd.read_csv(CSV_PATH, encoding='cp949')
    df['차종'] = df['차종'].astype(str).str.upper()
    results = df[df['차종'].str.contains(term, na=False)]['차종'].dropna().unique().tolist()[:10]
    return jsonify(results)

@app.route('/related_alc')
def related_alc():
    part_name = request.args.get("part_name", "").upper()
    df = pd.read_csv(CSV_PATH, encoding='cp949')
    df['부품명'] = df['부품명'].astype(str).str.upper()
    df['ALC'] = df['ALC'].astype(str).str.upper()
    results = df[df['부품명'] == part_name]['ALC'].dropna().unique().tolist()
    return jsonify(results)

if __name__ == '__main__':
    app.run(debug=True)
