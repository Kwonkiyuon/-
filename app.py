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
