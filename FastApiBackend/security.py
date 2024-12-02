import os
# 파일 암호화
from cryptography.hazmat.primitives import serialization, hashes
from cryptography.hazmat.primitives.asymmetric import padding
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
# 데이터 전송을 위한 base64
import base64

def encrypt(summary_result: str, public_key_pem: str):
    # 공개키 로드
    # print("공개키 정보: ", public_key_pem)
    public_key = serialization.load_pem_public_key(public_key_pem.encode())

    # AES 대칭키 생성
    aes_key = os.urandom(32)  # 256비트 키
    iv = os.urandom(12)  # GCM 모드에 사용할 IV

    # 데이터 암호화 (AES 사용)
    cipher = Cipher(algorithms.AES(aes_key), modes.GCM(iv))
    encryptor = cipher.encryptor()
    ciphertext = encryptor.update(summary_result.encode()) + encryptor.finalize()
    tag = encryptor.tag  # 생성된 인증 태그
    
    # 대칭키를 클라이언트의 공개키로 암호화
    encrypted_aes_key = public_key.encrypt(
        aes_key,
        padding.OAEP(
            mgf=padding.MGF1(algorithm=hashes.SHA256()),
            algorithm=hashes.SHA256(),
            label=None
        )
    )

    # 암호화된 데이터 및 대칭키를 base64 인코딩
    encoded_ciphertext = base64.b64encode(ciphertext).decode('utf-8')
    encoded_encrypted_aes_key = base64.b64encode(encrypted_aes_key).decode('utf-8')
    encoded_iv = base64.b64encode(iv).decode('utf-8')
    encoded_tag = base64.b64encode(tag).decode('utf-8')

    # 1. 암호화된 데이터(encoded_ciphertext)
    # 2. 클라이언트의 공개키로 암호화된 대칭키(encoded_encrypted_aes_key)
    # 3. AES-GCM 모드를 사용할 때, 암호화와 복호화에 사용하는 IV가 동일해야 합니다.
    # 4. 인증 태그
    return encoded_ciphertext, encoded_encrypted_aes_key, encoded_iv, encoded_tag