chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.action === 'translate') {
        // 獲取當前頁面的 HTML 和 Text
        chrome.scripting.executeScript({
            target: { tabId: request.tabId },
            func: () => {
				let htmlNodes = document.getElementsByTagName("*");
				let textNodes = [];
				let index = 0;
				for(var i = 0; i < htmlNodes.length; i++) {
					var el = htmlNodes[i];
					if(el.nodeName == "SCRIPT" || el.nodeName == "STYLE"){
						continue;
					}
					for (var j = 0; j < el.childNodes.length; j++) {
						var node = el.childNodes[j];
						if (node.nodeType === 3 && node.data.trim() != ''){
							let keyStr = "{#" + index + "}";
							textNodes.push({[keyStr]:node.data.trim()});
							node.data = keyStr;
							index += 1;
							continue; 
						}
					}
				}
				//console.log(textNodes);
                return {
					textNodes: textNodes
                };
            }
        }, (results) => {
            let textNodes = results[0].result;
			console.log(textNodes);
			// 使用 fetch API 向後端 API 發送請求
			fetch("https://localhost:7010/api/Translation/translate", {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json'
				},
				body: JSON.stringify({
					TextNodes: textNodes
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