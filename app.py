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

# 메모리 최적화를 위해 앱 시작 시 CSV를 한 번만 불러오기
DF_ORIGINAL = pd.read_csv(CSV_PATH, encoding='cp949')
DF_ORIGINAL.columns = DF_ORIGINAL.columns.str.strip().str.replace('"', '')
DF_ORIGINAL['part order done date'] = pd.to_datetime(DF_ORIGINAL['part order done date'], errors='coerce')
DF_ORIGINAL['ALC'] = DF_ORIGINAL['ALC'].astype(str).str.upper()
DF_ORIGINAL['부품명'] = DF_ORIGINAL['부품명'].astype(str).str.upper()
DF_ORIGINAL['차종'] = DF_ORIGINAL['차종'].astype(str).str.upper()
DF_ORIGINAL['부품 번호'] = DF_ORIGINAL['부품 번호'].astype(str).str.upper()

HTML_TEMPLATE = """
<!DOCTYPE html>
<html lang=\"ko\">
<head>
    <meta charset=\"UTF-8\">
    <meta http-equiv=\"refresh\" content=\"300\">
    <title>다차종 일일 생산량 조회</title>
    <script src=\"https://code.jquery.com/jquery-3.6.0.min.js\"></script>
    <link href=\"https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css\" rel=\"stylesheet\" />
    <script src=\"https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js\"></script>
    <style>
        body { font-family: Arial, sans-serif; padding: 20px; }
        .top-right { position: absolute; top: 10px; right: 10px; }
        table { border-collapse: collapse; width: 100%; margin-top: 20px; }
        th, td { border: 1px solid #ccc; padding: 8px; text-align: center; }
        th { background-color: #f2f2f2; }
        input[type=date], select { padding: 5px; margin-right: 10px; }
        button { padding: 5px 10px; }
        .error { color: red; margin-top: 20px; font-weight: bold; }
        .notice { margin-top: 20px; color: red; font-size: 20px; font-weight: bold; }
    </style>
    <script>
        $(function() {
            function updateALC(partName, model) {
                $.getJSON("/related_alc", { part_name: partName, model: model }, function(data) {
                    var alcSelect = $('#alc');
                    alcSelect.empty();
                    alcSelect.append($('<option>', { value: '', text: '선택 안함' }));
                    $.each(data, function(i, item) {
                        alcSelect.append($('<option>', { value: item, text: item }));
                    });
                });
            }

            $('#part_name').select2({
                ajax: {
                    url: '/autocomplete_part_name',
                    dataType: 'json',
                    delay: 250,
                    data: function(params) { return { term: params.term }; },
                    processResults: function(data) { return { results: $.map(data, function(item) { return { id: item, text: item }; }) }; },
                    cache: true
                },
                placeholder: '부품명을 입력하세요',
                minimumInputLength: 1
            }).on('select2:select', function (e) {
                updateALC(e.params.data.id, $('#model').val());
            });

            $('#model').select2({
                ajax: {
                    url: '/autocomplete_model',
                    dataType: 'json',
                    delay: 250,
                    data: function(params) { return { term: params.term }; },
                    processResults: function(data) { return { results: $.map(data, function(item) { return { id: item, text: item }; }) }; },
                    cache: true
                },
                placeholder: '차종을 입력하세요',
                minimumInputLength: 1
            });

            $('#alc').select2();
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
        차종: <select id=\"model\" name=\"model\"></select>
        부품명 : <select id=\"part_name\" name=\"part_name\"></select>
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

# 이하 Flask 라우트 함수는 그대로 유지
