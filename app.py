import os
import requests
import pandas as pd
from flask import Flask, render_template_string, request, jsonify

# Flask 앱 생성
app = Flask(__name__)

# CSV 파일 경로 및 자동 다운로드
CSV_URL = "https://github.com/kwonkiyuon/-/releases/download/v1.1/default.csv"
CSV_PATH = "/tmp/default.csv"

if not os.path.exists(CSV_PATH):
    r = requests.get(CSV_URL)
    with open(CSV_PATH, 'wb') as f:
        f.write(r.content)

# CSV 불러오기 및 전처리
DF_ORIGINAL = pd.read_csv(CSV_PATH, encoding='cp949')
DF_ORIGINAL.columns = DF_ORIGINAL.columns.str.strip().str.replace('"', '')
DF_ORIGINAL['part order done date'] = pd.to_datetime(DF_ORIGINAL['part order done date'], errors='coerce')
DF_ORIGINAL['ALC'] = DF_ORIGINAL['ALC'].astype(str).str.upper()
DF_ORIGINAL['부품명'] = DF_ORIGINAL['부품명'].astype(str).str.upper()
DF_ORIGINAL['차종'] = DF_ORIGINAL['차종'].astype(str).str.upper()
DF_ORIGINAL['부품 번호'] = DF_ORIGINAL['부품 번호'].astype(str).str.upper()

# HTML 템플릿 정의
HTML_TEMPLATE = """
<!doctype html>
<html>
<head>
    <title>다차종 일일 생산량 조회</title>
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
</head>
<body>
    <h2>다차종 일일 생산량 조회</h2>
    <form method="get">
        시작 날짜: <input type="date" name="start_date" value="{{ request.args.get('start_date', '') }}">
        종료 날짜: <input type="date" name="end_date" value="{{ request.args.get('end_date', '') }}">
        차종: <select id="model" name="model" style="width:200px"></select>
        부품명: <select id="part_name" name="part_name" style="width:300px"></select>
        ALC 코드: <select id="alc" name="alc">
            <option value="">선택 안함</option>
            {% for a in alcs %}
                <option value="{{ a }}" {% if a == request.args.get('alc', '') %}selected{% endif %}>{{ a }}</option>
            {% endfor %}
        </select>
        <button type="submit">조회</button>
    </form>

    {% if error %}<p style="color:red">{{ error }}</p>{% endif %}
    {% if data is not none %}
        <table border="1">
            <thead><tr>{% for col in data.columns %}<th>{{ col }}</th>{% endfor %}</tr></thead>
            <tbody>
            {% for row in data.itertuples() %}
                <tr>{% for value in row[1:] %}<td>{{ value }}</td>{% endfor %}</tr>
            {% endfor %}
            </tbody>
        </table>
    {% endif %}

    <script>
        function initSelect2(id, url, defaultValue) {
            $(id).select2({
                ajax: {
                    url: url,
                    dataType: 'json',
                    delay: 250,
                    data: function (params) {
                        return { term: params.term };
                    },
                    processResults: function (data) {
                        return { results: data.map(d => ({ id: d, text: d })) };
                    }
                },
                placeholder: '검색 또는 선택...',
                allowClear: true
            });
            if (defaultValue) {
                var option = new Option(defaultValue, defaultValue, true, true);
                $(id).append(option).trigger('change');
            }
        }

        $(document).ready(function () {
            initSelect2('#part_name', '/autocomplete_part_name', '{{ request.args.get('part_name', '') }}');
            initSelect2('#model', '/autocomplete_model', '{{ request.args.get('model', '') }}');

            $('#part_name, #model').on('change', function () {
                var part = $('#part_name').val();
                var model = $('#model').val();
                if (part && model) {
                    $.getJSON('/related_alc', { part_name: part, model: model }, function (data) {
                        $('#alc').empty().append('<option value="">선택 안함</option>');
                        data.forEach(function (item) {
                            $('#alc').append('<option value="' + item + '">' + item + '</option>');
                        });
                    });
                }
            });
        });
    </script>
</body>
</html>
"""

# index 라우트
@app.route("/")
def index():
    start_date = request.args.get('start_date')
    end_date = request.args.get('end_date')
    model = request.args.get('model', '').upper()
    part_name = request.args.get('part_name', '').upper()
    alc = request.args.get('alc', '').upper()

    df = DF_ORIGINAL.copy()
    error = None

    if not start_date or not end_date:
        error = "시작 날짜와 종료 날짜는 필수입니다."
        alcs = []
        return render_template_string(HTML_TEMPLATE, data=None, alcs=alcs, error=error)

    df = df[(df['part order done date'] >= start_date) & (df['part order done date'] <= end_date)]

    if alc:
        df = df[df['ALC'] == alc]
    elif part_name:
        df = df[df['부품명'] == part_name]
        if model:
            df = df[df['차종'] == model]
    elif model:
        df = df[df['차종'] == model]

    alcs = df['ALC'].unique().tolist()
    return render_template_string(HTML_TEMPLATE, data=df, alcs=alcs, error=error)

@app.route('/autocomplete_part_name')
def autocomplete_part_name():
    term = request.args.get('term', '').upper()
    matches = DF_ORIGINAL[DF_ORIGINAL['부품명'].str.contains(term, na=False)]['부품명'].unique().tolist()
    return jsonify(sorted(matches))

@app.route('/autocomplete_model')
def autocomplete_model():
    term = request.args.get('term', '').upper()
    matches = DF_ORIGINAL[DF_ORIGINAL['차종'].str.contains(term, na=False)]['차종'].unique().tolist()
    return jsonify(sorted(matches))

@app.route('/related_alc')
def related_alc():
    part_name = request.args.get('part_name', '').upper()
    model = request.args.get('model', '').upper()
    filtered_df = DF_ORIGINAL[(DF_ORIGINAL['부품명'] == part_name) & (DF_ORIGINAL['차종'] == model)]
    alcs = filtered_df['ALC'].unique().tolist()
    return jsonify(sorted(alcs))
