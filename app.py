플라스크 가져오기 플라스크, render_template_string, 요청
sqlite3 가져오기
판다를 PD로 가져오기
os 가져오기
가져오기 요청

앱 = 플라스크(__name__)

DB_URL = "https://github.com/Kwonkiyuon/-/releases/download/v1.0/default.db "
DB_PATH = "default.db"

# DB 가 없으면 GitHub, 에서 다운로드 출시
그렇지 않은 경우 os.path.exists(DB_PATH):
    인쇄("DB 파일이 없습니다. GitHub에서 다운로드 중...")
 응답 = requests.get(DB_URL)
 open(DB_PATH, "wb")을 f로 사용합니다:
 f.write(응답.content)
    인쇄("DB 다운로드 완료!")

HTML_TEMPLATE = ""
<!DOCTYPE HTML>
<html>
<머리>
 <title>다차종 생산량 뷰어</title>
 <스타일>
 본문 {폰트 패밀리}: 에어리얼, 산세리프; 패딩: 20px; }
 테이블 {경계-collapse: 붕괴; 너비: 100%; 여백 상단: 20 px; }
 td {경계}: 1 px 솔리드 #ccc; 패딩: 8 px; 텍스트 align: 왼쪽; }
 th { background-color: #f2f2f2; }
 입력[type=text], 입력[type=date] {패딩: 5 px; 여백-오른쪽: 10 px; }
 .오류 {색: 빨간색; 여백 상단: px 10개; }
 </스타일>
</머리>
<바디>
 <h1>다차종 일일 생산량 조회</h1>
 <form method='get'>
 날짜(완료 날짜): <입력 유형='날짜' 이름='날짜' 값='{{request.args.get ("date", "")}}'>
 부품 번호: <입력 유형='text' name='part_no' 값='{{request.args.get ("part_no", "")}}'>
 ALC 코드: <입력 유형='텍스트' 이름='alc' 값='{{request.args.get ("alc", "")}}'>
 <버튼 유형='submit'>조회</버튼>
 </형식>
 {% 오류 %}인 경우
 <p class='error'>{{오류 }}/p>
 {% 끝이 %}인 경우
 {% 데이터가 있는 경우.모양 [0] > 0 %}
 <p><강한>총 생산 수량: {{데이터}.모양[0]} EA </strong></p>
 <표>
 콜린 데이터의 경우 <tr>{%}.columns %} 워싱 {{ 콜}}/th>{% 끝: %}/tr>
 {% _의 경우, data.iterrows () %} 행
 콜린 데이터의 경우 <tr>{%}<columns %}<td>{{row[col]}}</%의 경우 td>{% 끝}</tr>
 {% %}에 대한 끝
 </표>
 {% 끝이 %}인 경우
</body>
</html>
"""

@app.route('/'
인덱스 () 정의:
 date_filter = request.args.get ('date, ')
    part_no_filter = request.args.get('part_no', '').upper()
    alc_filter = request.args.get('alc', '').upper()
    error = ''

    query = "SELECT * FROM 생산량 WHERE 1=1"
    params = []

    if not (date_filter or part_no_filter or alc_filter):
        df = pd.DataFrame(columns=["ID", "바디넘버", "공정", "부품명", "부품 번호", "ALC", "인덱스값", "part order start date", "part order done date", "차종"])
        error = '조회하려면 날짜나 ALC 코드를 입력해주세요.'
    else:
        if alc_filter:
            query += " AND UPPER(ALC) LIKE ?"
            params.append(f"%{alc_filter}%")
        if part_no_filter:
            query += " AND UPPER(부품_번호) LIKE ?"
            params.append(f"%{part_no_filter}%")
        if date_filter:
            query += " AND part_order_done_date LIKE ?"
            params.append(f"{date_filter}%")

        conn = sqlite3.connect(DB_PATH)
        df = pd.read_sql_query(query, conn, params=params)
        conn.close()

    return render_template_string(HTML_TEMPLATE, data=df, request=request, error=error)

if __name__ == '__main__':
    app.run(debug=True)
