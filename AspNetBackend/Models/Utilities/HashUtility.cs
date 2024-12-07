using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace AspNetBackend.Models.Utilities
{
    public class HashUtility
    {
        public static string GenerateFileHash(HttpPostedFileBase pdfDoc)
        {
            using (var sha256 = SHA256.Create())
            {
                // PDF 파일 내용을 바이트 배열로 읽기
                byte[] fileBytes = new byte[pdfDoc.InputStream.Length];
                pdfDoc.InputStream.Read(fileBytes, 0, fileBytes.Length);
                pdfDoc.InputStream.Position = 0; // 스트림 위치 초기화

                // SHA256 해시 값 생성
                var hashBytes = sha256.ComputeHash(fileBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower(); // 문자열로 변환
            }
        }
    }
}