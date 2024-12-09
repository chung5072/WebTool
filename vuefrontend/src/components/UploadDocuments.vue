<script setup>
	// import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
	import { ref, reactive, onMounted } from 'vue';
	import axios from 'axios';
	import forge from 'node-forge'; // 공개키, 비밀키 암호화

	// Model: 문서 관련 데이터와 상태를 관리
	// 클라이언트 ID를 반응형 상태로 저장
	const publicKeyPem = ref(null);
	// 비밀키 저장
	const privateKeyPem = ref(null);

	const documentModel = reactive({
		pdfDoc: {
				file: null,
				url: null,
		},
		result: {
				reqDoc: null,
				resDoc: null
		},
		hasAnalysisResult: false,
	});

    // Vue.js가 마운트될 때 공개키를 생성하고 서버로 전송
    onMounted(() => {
        // 공개키 및 비밀키 생성
        const keypair = forge.pki.rsa.generateKeyPair(2048);
        publicKeyPem.value = forge.pki.publicKeyToPem(keypair.publicKey);
        privateKeyPem.value = forge.pki.privateKeyToPem(keypair.privateKey);
    });

		// ViewModel: 모델과 뷰 사이의 로직을 처리
		async function handleFileUpload(files) {
			const file = files[0];
			documentModel.pdfDoc.file = file;
		}

		// FastAPI에서 반환한 파일 경로를 사용
		function fetchAnalysisResult(summarizedResult, decryptionKey, encryptionInitialState, authTag) {
			try {
				// base64 디코딩
				const encodedData = atob(summarizedResult);
				// console.log("인코딩된 데이터 ", encodedData);
				const encodedInitialState = atob(encryptionInitialState);
				const encodedTag = atob(authTag);

				// Uint8Array로 변환
				const decodedDataArray = new Uint8Array(encodedData.split('').map(char => char.charCodeAt(0)));
				// console.log("디코딩된 데이터 ", decodedDataArray);
				const decodedInitialStateArray = new Uint8Array(encodedInitialState.split('').map(char => char.charCodeAt(0)));
				const decodedTagArray = new Uint8Array(encodedTag.split('').map(char => char.charCodeAt(0)));

				// 비밀키 로드
				const privateKey = forge.pki.privateKeyFromPem(privateKeyPem.value);

				try {
				// RSA-OAEP를 사용하여 대칭키 복호화
				const aesKey = privateKey.decrypt(atob(decryptionKey), 'RSA-OAEP', {
					md: forge.md.sha256.create(),
					mgf1: { md: forge.md.sha256.create() },
				});

				// 대칭키를 사용하여 데이터 복호화
				const decipher = forge.cipher.createDecipher('AES-GCM', aesKey);
				// 초기화 벡터 및 인증 태그
				decipher.start({
					iv: decodedInitialStateArray,
					tag: decodedTagArray,
				});

				decipher.update(forge.util.createBuffer(decodedDataArray));

				if (!decipher.finish()) {
					throw new Error('[에러] 대칭키 복호화 실패 - 요약 결과');
				}

				documentModel.result.resDoc = forge.util.decodeUtf8(decipher.output.getBytes());

				documentModel.hasAnalysisResult = true;
				} catch (error) {
				console.error("[에러] 비밀키 복호화 실패 - 대칭키: ", error);
				}
			} catch (error) {
				console.error("[에러] Base64 디코딩 실패 - 암호화된 데이터 혹은 대칭키 생성시 필요 정보: ", error);
			}
		}

		function goToAnalysisPage() {
			const formData = new FormData();
			// 암호화할 공개키
			formData.append('publicKeyPem', publicKeyPem.value);
			// pdf 문서
			formData.append('pdfDoc', documentModel.pdfDoc.file);

			axios.post('https://localhost:44363/api/document/upload-pdf-doc', formData, {
				headers: {
					'Content-Type': 'multipart/form-data',
				},
			})
			.then((response) => {
				// console.log("업로드 요청: ", response)
				// 분석 결과 파일 경로를 받아서 fetchAnalysisResult 호출
				//console.log("파일 이름: ", response.data.ResultDocName);
				fetchAnalysisResult(
					response.data.ResultSummarizedContent,
					response.data.DecryptionKey,
					response.data.EncryptionInitialState,
					response.data.AuthTag
				);
			})
			.catch((error) => {
				console.error('[에러] PDF 파일 업로딩 혹은 PDF 요약: ', error);
			});
		}

// .txt 파일 다운로드 기능 추가
function downloadAsTxt() {
  if (!documentModel.result.resDoc) return;

  // 업로드된 파일 이름에서 확장자 제거
  const originalFileName = documentModel.pdfDoc.file.name;
  const fileNameWithoutExtension = originalFileName.substring(0, originalFileName.lastIndexOf('.')) || originalFileName;

  // 새로운 파일 이름 생성
  const summarizedFileName = `${fileNameWithoutExtension}_요약결과.txt`;

  // Blob 생성
  const blob = new Blob([documentModel.result.resDoc], { type: 'text/plain' });

  // URL 생성
  const url = URL.createObjectURL(blob);

  // 가상 링크 생성 및 클릭
  const link = document.createElement('a');
  link.href = url;
  link.download = summarizedFileName; // 다운로드 파일 이름 설정
  link.click();

  // URL 해제
  URL.revokeObjectURL(url);
}
</script>

<template>
	<!-- View: 사용자 인터페이스 정의 -->
	<div>
		<!-- PDF Upload -->
		<div>
			<input type="file" @change="handleFileUpload($event.target.files)" accept=".pdf" />
			<div v-if="documentModel.pdfDoc.file">
				<button @click="goToAnalysisPage">Analyze</button>
			</div>
		</div>

		<!-- Analysis Result -->
		<div v-if="documentModel.hasAnalysisResult">
			<p>Analysis Page</p>
			<p>{{ documentModel.result.resDoc }}</p>
      <button @click="downloadAsTxt">결과 다운로드</button>
		</div>
	</div>
</template>
