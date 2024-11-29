<script setup>
		import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
		import { ref, reactive } from 'vue';
		import axios from 'axios';

		// Model: 이미지 관련 데이터와 상태를 관리
		const imageModel = reactive({
				prev: {
						file: null,
						url: null,
				},
				after: {
						file: null,
						url: null,
				},
				resultUrls: {
						prev: null,
						after: null,
				},
				isComparePage: false,
		});

		// ViewModel: 모델과 뷰 사이의 로직을 처리
		const prevImageIcon = ref(null);
		const afterImageIcon = ref(null);

		function uploadPrevImage() {
				prevImageIcon.value.click();
				imageModel.isComparePage = false;
		}

		function uploadAfterImage() {
				afterImageIcon.value.click();
				imageModel.isComparePage = false;
		}

		async function handleFileUpload(files, type) {
				const file = files[0];
				if (type === 'prev') {
						imageModel.prev.file = file;
						await convertToBase64(file, 'prev');
				} else if (type === 'after') {
						imageModel.after.file = file;
						await convertToBase64(file, 'after');
				}
		}

		function convertToBase64(file, type) {
				return new Promise((resolve) => {
						const reader = new FileReader();
						reader.onload = (e) => {
								if (type === 'prev') {
										imageModel.prev.url = e.target.result;
								} else if (type === 'after') {
										imageModel.after.url = e.target.result;
								}
								resolve(e.target.result);
						};
						reader.readAsDataURL(file);
				});
		}

		function goToComparePage() {
				const formData = new FormData();
				formData.append('prevImage', imageModel.prev.file);
				formData.append('afterImage', imageModel.after.file);

				axios.post('https://localhost:44363/api/images/upload', formData, {
						headers: {
								'Content-Type': 'multipart/form-data',
						},
				})
						.then((response) => {
								console.log(response);
								imageModel.resultUrls.prev = response.data.PrevImageUrl;
								imageModel.resultUrls.after = response.data.AfterImageUrl;
								imageModel.isComparePage = true;
						})
						.catch((error) => {
								console.error('Error uploading images:', error);
						});
		}
</script>

<template>
		<!-- View: 사용자 인터페이스 정의 -->
		<div>
				<!-- Prev 이미지 업로드 -->
				<div>
						<div v-if="imageModel.prev.url" @click="uploadPrevImage">
								<img :src="imageModel.prev.url" alt="Image Preview" style="max-width: 100%; max-height: 300px; cursor: pointer;" />
						</div>
					<div v-else>
							<button @click="uploadPrevImage">
									<font-awesome-icon icon="file-import" size="6x" />
							</button>
					</div>
					<input type="file" ref="prevImageIcon" @change="handleFileUpload($event.target.files, 'prev')" hidden />
				</div>

				<!-- After 이미지 업로드 -->
				<div>
						<div v-if="imageModel.after.url" @click="uploadAfterImage">
								<img :src="imageModel.after.url" alt="Image Preview" style="max-width: 100%; max-height: 300px; cursor: pointer;" />
						</div>
						<div v-else>
								<button @click="uploadAfterImage">
										<font-awesome-icon icon="file-import" size="6x" />
								</button>
						</div>
						<input type="file" ref="afterImageIcon" @change="handleFileUpload($event.target.files, 'after')" hidden />
				</div>

				<!-- 이미지 둘 다 업로드가 됐다면 비교 페이지로 이동 -->
				<div v-if="imageModel.prev.url && imageModel.after.url">
						<button @click="goToComparePage">
								<p>비교</p>
						</button>
				</div>
		</div>

		<!-- 비교한 이미지 결과를 보여주는 부분 -->
		<div v-if="imageModel.isComparePage">
				<p>비교 페이지</p>
				<!-- 여기에 비교하는 컴포넌트나 내용을 추가 -->
				<p>{{ imageModel.resultUrls.prev }}</p>
				<p>{{ imageModel.resultUrls.after }}</p>
		</div>
</template>
