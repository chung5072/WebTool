from fastapi import FastAPI
# 데이터를 받기 위해 모델 생성
from pydantic import BaseModel

# .Net MVC 5 -> FastAPI
class Images(BaseModel):
    prevImagePath: str
    afterImagePath: str

# .Net MVC 5 <- FastAPI
class CompareResult(BaseModel):
    IsSuccess: bool
    result: str

app = FastAPI()

@app.post("/compare")
def read_root(images: Images):
    print("이전 이미지: ", images.prevImagePath)
    print("이후 이미지: ", images.afterImagePath)
    return CompareResult(IsSuccess=True, result="실력이 늘고 있습니다.");