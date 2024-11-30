from fastapi import FastAPI, HTTPException, Request
from fastapi.exceptions import RequestValidationError
from fastapi.responses import JSONResponse
from fastapi.staticfiles import StaticFiles
from fastapi.middleware.cors import CORSMiddleware
# 데이터를 받기 위해 모델 생성
from pydantic import BaseModel
# 분석 결과를 파일로 저장할 수 있도록
import os
# 파일 이름 랜덤 생성
import uuid
# 파일 암호화
from cryptography.hazmat.primitives import serialization, hashes
from cryptography.hazmat.primitives.asymmetric import padding
# Import base64 module
import base64  

# .Net MVC 5 -> FastAPI
# 분석할 pdf 문서
class PdfDocument(BaseModel):
    public_key_pem: str
    pdf_doc_path: str

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:50142"],  # Vue.js 개발 서버 주소
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# 'result' 디렉토리를 '/static' 경로로 마운트
app.mount("/static", StaticFiles(directory="result"), name="static")

@app.exception_handler(RequestValidationError)
async def validation_exception_handler(request: Request, exc: RequestValidationError):
    body = await request.body()
    body_text = body.decode('utf-8')  # 바이트를 문자열로 디코드
    # 유효성 검사 오류를 로그에 기록
    print(f"Validation error for request {request.url}: {exc} with body {body_text}")
    # 클라이언트에게 오류 응답 반환
    return JSONResponse(
        status_code=422,
        content={"detail": exc.errors()},
    )

@app.post("/api/analyze")
async def analyze_pdf(pdf_doc: PdfDocument):
    print("pdf 파일 위치: ", pdf_doc.pdf_doc_path)

    # PDF 분석 로직 추가
    analysis_result = "아 이제 암복호화 됐나보다."
    
    # 공개키 로드
    public_key = serialization.load_pem_public_key(pdf_doc.public_key_pem.encode())
    
    # 분석 결과 암호화
    encrypted_result = public_key.encrypt(
        analysis_result.encode(),
        padding.OAEP(
            mgf=padding.MGF1(algorithm=hashes.SHA256()),
            algorithm=hashes.SHA256(),
            label=None
        )
    )

    # txt파일에 담기 위해 base64 인코딩
    encoded_result = base64.b64encode(encrypted_result).decode('utf-8')

    # 현재 디렉토리 기준으로 절대 경로 생성
    # 저장할 디렉토리 경로
    directory = "result"
    # 랜덤 파일 이름 생성
    file_name = f"{uuid.uuid4()}.txt"
    file_path = os.path.join(directory, file_name)

    # 디렉토리가 존재하지 않으면 생성
    if not os.path.exists(directory):
        os.makedirs(directory)

    with open(file_path, "w", encoding="utf-8") as analysis_result_file:
        analysis_result_file.write(encoded_result)

    return file_name