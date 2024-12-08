from fastapi import FastAPI, Request, UploadFile, File
from fastapi.exceptions import RequestValidationError
from fastapi.responses import JSONResponse
# from fastapi.staticfiles import StaticFiles
from fastapi.middleware.cors import CORSMiddleware

# 데이터를 받기 위해 모델 생성
from pydantic import BaseModel

# 분석 결과를 파일로 저장할 수 있도록
# 파일 관리
import os
# 파일 이름 랜덤 생성
# import uuid
# 파일 분석 - 요약
from analyze import summary_pdf
# 파일 암호화
from security import encrypt

# 클라우드 스토리지
from azure.storage.blob import BlobServiceClient

# 환경 변수 설정
from dotenv import load_dotenv

# 로그
# import logging

# logging.basicConfig(level=logging.INFO)

# .env 파일 로드
load_dotenv()

# .Net MVC 5 -> FastAPI
# 분석할 pdf 문서
class PdfDocument(BaseModel):
    PdfDocPath: str
    PublicKeyPem: str

# .Net MVC 5 <- FastAPI
# 분석 결과가 저장된 문서
class SummarizedDocument(BaseModel):
    # ResultDocName: str
    ResultSummarizedContent: str
    DecryptionKey: str
    EncryptionInitialState: str
    AuthTag: str

# Azure Blob Storage 설정
connect_str = os.getenv("AZURE_CONNECTION_STRING")
blob_service_client = BlobServiceClient.from_connection_string(connect_str)
container_name = os.getenv("AZURE_CONTAINER_NAME")

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    # vue.js 및 asp.net mvc 5 개발 주소
    allow_origins=os.getenv("ALLOWED_ORIGINS").split(','), 
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# 'result' 디렉토리를 '/static' 경로로 마운트
# app.mount("/static", StaticFiles(directory="result"), name="static")

# 요청에 대해 발생하는 에러를 로그로 기록
@app.exception_handler(RequestValidationError)
async def validation_exception_handler(request: Request, exc: RequestValidationError):
    body = await request.body()
    # 바이트를 문자열로 디코드
    body_text = body.decode('utf-8')  
    # 유효성 검사 오류를 로그에 기록
    print(f"Validation error for request {request.url}: {exc} with body {body_text}")
    # 클라이언트에게 오류 응답 반환
    return JSONResponse(
        status_code=422,
        content={"detail": exc.errors()},
    )

@app.post("/api/summary-pdf")
async def summary(pdf_doc: PdfDocument):
    # 사용 예시
    # print("저장된 pdf 파일 위치: ", pdf_doc.PdfDocPath)

    # Azure Blob Storage에서 파일 다운로드
    blob_client = blob_service_client.get_blob_client(container=container_name, blob=pdf_doc.PdfDocPath)
    download_stream = blob_client.download_blob()
    pdf_content = download_stream.readall()

    # PDF 분석 로직 
    # 테스트 값
    # summary_result = "테스트 결과"
    # 로컬 저장소
    # summary_result = summary_pdf(pdf_doc.PdfDocPath)
    # 클라우드 저장소
    summary_result = summary_pdf(pdf_content)
    # print("pdf문서 전체 요약 결과: ", summary_result)
    
    # 파일 암호화
    encoded_summarized_text, encoded_key, encoded_initial_state, encoded_tag = encrypt(summary_result, pdf_doc.PublicKeyPem)
    # print("인코딩된 대칭키", encoded_encrypted_key)

    # 랜덤 파일 이름 생성
    # file_name = f"{uuid.uuid4()}.txt"

    # 로컬 저장소
    # 현재 디렉토리 기준으로 절대 경로 생성
    # 저장할 디렉토리 경로
    # directory = "result"
    # file_path = os.path.join(directory, file_name)

    # # 디렉토리가 존재하지 않으면 생성
    # if not os.path.exists(directory):
    #     os.makedirs(directory)

    # with open(file_path, "w", encoding="utf-8") as analysis_result_file:
    #     analysis_result_file.write(encoded_summarized_text)

    # 클라우드 저장소
    # 결과를 Azure Blob Storage에 업로드
    # result_blob_client = blob_service_client.get_blob_client(container=container_name, blob=f"results/{file_name}")
    # result_blob_client.upload_blob(encoded_summarized_text)

    return SummarizedDocument(
        ResultSummarizedContent = encoded_summarized_text,
        DecryptionKey = encoded_key,
        EncryptionInitialState = encoded_initial_state,
        AuthTag = encoded_tag
    )