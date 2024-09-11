chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.action === 'translate') {
        // 獲取當前頁面的 HTML 和 Text
        chrome.scripting.executeScript({
            target: { tabId: request.tabId },
            func: () => {
                return {
                    htmlContent: document.body.innerHTML,
                    textContent: document.body.innerText,
					documentBody: document.body
                };
            }
        }, (results) => {
            let { htmlContent, textContent } = results[0].result;
			// 使用 fetch API 向後端 API 發送請求
			fetch("https://localhost:7010/api/Translation/translate", {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json'
				},
				body: JSON.stringify({
					HtmlContent: htmlContent,
					TextContent: textContent
				})
			}).then(response => response.json())
			.then(data => {
				console.log(data);
				sendResponse(data);
			})
			.catch((error) => {
				console.error('Error:', error);
			});
        });

        // 需要返回 true 以允許異步響應
        return true;
    }
});