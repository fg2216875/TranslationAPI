// popup.js
document.getElementById('replaceButton').addEventListener('click', function() {
	console.log("AAA");
	// 獲取當前的活動標籤
    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        let activeTab = tabs[0];
		// 先注入 axios.js
        chrome.scripting.executeScript({
            target: { tabId: activeTab.id },
            files: ['Scripts/axios.js'] // 確保 axios.min.js 文件在擴充套件的目錄中
        }, () => {
            // 然後注入 replaceContent 函數
            chrome.scripting.executeScript({
                target: { tabId: activeTab.id },
                func: replaceContent
            });
        });
    });
	
});

function replaceContent() {	
	// 獲取當前頁面的 HTML 和 Text
	let htmlContent = document.body.innerHTML;
	let textContent = document.body.innerText;
	console.log("BBBB");
	//document.body.innerHTML = '<h1>這是新的頁面內容</h1>';
	// 向後端 API 發送請求
	axios.post("https://localhost:7010/api/Translation/translate",{
		HtmlContent: htmlContent,
		TextContent: textContent
	}).then((response) => {
		console.log(response);
		// 使用翻譯後的 HTML 替換當前頁面的內容
		document.body.innerHTML = response.data.TranslatedHtmlContent;
		console.log("replaceContent function executed.");
	})
	.catch((error) => {
		console.error('Error:', error);
	});
}


// function replaceContent() {	
	// axios.post("https://localhost:7010/api/Translation/translate",{
		// HtmlContent:document.body.innerHTML,
		// TextContent:document.body.innerText
	// }).then((response) => {
		// console.log(response);
		// document.body.innerHTML = response.data.TranslatedHtmlContent;
		// console.log("replaceContent function executed.");
	// })
// }

// document.getElementById("replaceButton").addEventListener("click", () => {
    // // 獲取當前的活動標籤
    // chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        // let activeTab = tabs[0];
        // // 在當前頁面執行替換內容的腳本
        // chrome.scripting.executeScript({
            // target: { tabId: activeTab.id },
            // function: replaceContent
        // });
    // });
// });

// 用於替換頁面內容的函數
// function replaceContent() {
    // document.body.innerHTML = '<h1>這是新的頁面內容</h1>';
// }