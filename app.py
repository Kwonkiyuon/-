플라스크 가져오기 플라스크, render_template_string, 요청
판다를 PD로 가져오기
sqlite3 가져오기

앱 = 플라스크(__name__)

# DB 경로
DB_PATH = "통합생산량.db"

# HTML 템플릿
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
 <form method="get">
 날짜(완료 날짜): <입력 유형="날짜" 이름="날짜" 값="{{request.args.get ('date,'''}}">
 부품 번호: <입력 유형="text" name="part_no" 값="{{request.args.get ('part_no", ''}}}">
 ALC 코드: <입력 유형="텍스트" 이름="alc" 값="{{request.args.get ('alc", ''}}}">
 <버튼 유형="submit">조회</버튼>
 </형식>

 {% 오류 %}인 경우
 <p class="error">{{오류 }}/p>
 {% 끝이 %}인 경우

 {% 데이터가 있는 경우.모양 [0] > 0 %}
 <p><강한>총 생산 수량: {{데이터}.모양[0]} EA </strong></p>
 <표>
 <tr>
 {% 콜린 데이터의 경우.columns %}
 {{ 콜}}/th>
 {% %}에 대한 끝
 </tr>
 {% _의 경우, data.iterrows () %} 행
 <tr>
 {% 콜린 데이터의 경우.columns %}
 <td>{{ row[col] }}</td>
 {% %}에 대한 끝
 </tr>
 {% %}에 대한 끝
 </표>
 {% 끝이 %}인 경우
</body>
</html>
"""

@app.route('/'
인덱스 () 정의:
    # DB에서 전체 테이블 로드
 conn = sqlite3.connect(DB_PATH)
 df = pd.read_sql("SELECT * FROM 프로덕션", conn)
 conn.close ()

    # 필터값 수집 및 대문자 변환
 date_filter = request.args.get ('date, ')
 part_no_filter = request.args.get ('part_no, ''). 상위 ()
 alc_filter = request.args.get ('alc', ''). 상위 ()

 오류 = ''

    # 입력값 없으면 빈 결과 + 에러 메시지
 그렇지 않은 경우(date_filter 또는 part_no_filter 또는 alc_filter):
 df = df.iloc[0:0]
 error = '조회하려면 날짜나 ALC 코드를 입력해주세요.'
 그렇지 않으면:
 만약 alc_filter:
 df = df[df]ALC'].astype(str.str.upper ().str.contains(alc_필터))]
 if part_no_filter:
 df = df['부품 번호'].astype(str.str.upper ().str.contains(part_no_filter))]
 만약 date_filter:
 df = df['부품 주문 완료 날짜'].타입(str.str.str.starts(date_filter))]

 return render_template_string (HTML_TEMPLATE, data=df, request=request, error=error)

__name__ == '__main__:
 app.run(디버그=True)
