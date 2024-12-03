from dotenv import load_dotenv
from langchain_community.document_loaders import PyPDFLoader
from transformers import AutoTokenizer, AutoModelForSeq2SeqLM
import nltk

import tempfile

# 환경 변수 설정 및 로드
load_dotenv()

# NLTK 데이터 다운로드 (필요한 경우)
# nltk.download('punkt_tab')

# T5 모델과 토크나이저 로드
MODEL_NAME = 'eenzeenee/t5-base-korean-summarization'
tokenizer = AutoTokenizer.from_pretrained(MODEL_NAME)
model = AutoModelForSeq2SeqLM.from_pretrained(MODEL_NAME)

# 로컬 저장소
# def summary_pdf(doc_path: str):
# 클라우드 저장소
def summary_pdf(pdf_bytes: bytes):
     # 임시 파일 생성
    with tempfile.NamedTemporaryFile(delete=False, suffix=".pdf") as temp_file:
        temp_file.write(pdf_bytes)
        temp_file_path = temp_file.name
    # PDF 파일 로드
    # 로컬 저장소
    # loader = PyPDFLoader(doc_path)
    # 클라우드 저장소
    loader = PyPDFLoader(temp_file_path)
    document = loader.load()

    # 모든 페이지의 내용을 하나의 문자열로 결합
    full_text = " ".join([page.page_content for page in document])

    # 텍스트를 적절한 길이로 분할하여 요약
    chunk_size = 512  # 각 청크의 최대 길이 (모델에 따라 조정 필요)
    overlap = 50  # 중복 부분 (선택 사항)

    # 각 청크를 요약하고 결과를 결합
    summaries = []
    prefix = "summarize: "
    for chunk in split_text(full_text, chunk_size, overlap):
        inputs = tokenizer(prefix + chunk, max_length=chunk_size, truncation=True, return_tensors="pt")
        output = model.generate(**inputs, num_beams=3, do_sample=True, min_length=10, max_length=64)
        decoded_output = tokenizer.batch_decode(output, skip_special_tokens=True)[0]
        result = nltk.sent_tokenize(decoded_output.strip())[0]
        summaries.append(result)

    # 요약본 통합
    final_summary = " ".join(summaries)

    # 최종 요약 결과 출력
    return final_summary

# 텍스트가 모델이 이해할 수 있는 길이로 분할
def split_text(text, chunk_size, overlap):
    start = 0
    while start < len(text):
        end = min(start + chunk_size, len(text))
        yield text[start:end]
        start += chunk_size - overlap

# 메서드 테스트
# print(analyze_pdf('data/test.pdf'))