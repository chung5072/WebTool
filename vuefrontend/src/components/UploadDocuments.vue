<script setup>
		import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
		import { ref, reactive, onMounted } from 'vue';
		import axios from 'axios';
		import forge from 'node-forge'; // 공개키, 비밀키 암호화

		// Model: 문서 관련 데이터와 상태를 관리
		// 클라이언트 ID를 반응형 상태로 저장
		const publicKeyPem = ref(null);
		// 비밀키 저장
		const privateKeyPem = ref(null);

		// Vue.js가 마운트될 때 공개키를 생성하고 서버로 전송
		onMounted(() => {
				// 공개키 생성
				const keypair = forge.pki.rsa.generateKeyPair(2048);
				publicKeyPem.value = forge.pki.publicKeyToPem(keypair.publicKey);
				privateKeyPem.value = forge.pki.privateKeyToPem(keypair.privateKey);
		});

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

		// ViewModel: 모델과 뷰 사이의 로직을 처리
		async function handleFileUpload(files) {
				const file = files[0];
				documentModel.pdfDoc.file = file;
		}

		// FastAPI에서 반환한 파일 경로를 사용
	function fetchAnalysisResult(filePath) {
				// 분석 결과가 저장된 정적 파일을 요청
				axios.get(`http://localhost:8000/static/${filePath}`)
					.then(res => {
							console.log("암호화된 초기 res: ", res);
							// Base64로 인코딩된 데이터를 디코딩 (JavaScript 내장 함수 사용)
						const base64DecodedData = atob(res.data);
						console.log("base64 디코드 data: ", base64DecodedData);
            // 디코딩된 문자열을 Uint8Array로 변환
            const encryptedData = new Uint8Array(base64DecodedData.split('').map(char => char.charCodeAt(0)));
            console.log("Uint8Array로 변환 data: ", encryptedData);
            // 비밀키 로드
            const privateKey = forge.pki.privateKeyFromPem(privateKeyPem.value);
            try {
              // RSA-OAEP를 사용하여 복호화
              const decryptedData = privateKey.decrypt(encryptedData, 'RSA-OAEP', {
                md: forge.md.sha256.create(),
                mgf1: {
                  md: forge.md.sha256.create()
                }
              });

              documentModel.result.resDoc = forge.util.decodeUtf8(decryptedData);
              documentModel.hasAnalysisResult = true;
            } catch (error) {
              console.error("복호화 중 오류 발생: ", error);
            }
					})
          .catch(e => {
							console.error(`ERROR ${e.response ? e.response.status : 'unknown'} : ${e.request ? e.request.responseURL : 'unknown'}`);
          });
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
								console.log(response);
							documentModel.result.reqDoc = response.data.pdfDocName;
							// 분석 결과 파일 경로를 받아서 fetchAnalysisResult 호출
							console.log("파일 이름: ", response.data.resultDocName);
              fetchAnalysisResult(response.data.resultDocName);
						})
						.catch((error) => {
								console.error('Error uploading PDF:', error);
						});
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
						<p>{{ documentModel.result.reqDoc }}</p>
						<pre>{{ documentModel.result.resDoc }}</pre> <!-- Use <pre> for preformatted text -->
				</div>
		</div>
</template>
