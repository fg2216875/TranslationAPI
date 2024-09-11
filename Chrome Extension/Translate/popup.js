document.getElementById('replaceButton').addEventListener('click', function() {
    console.log("AAA");
    // 獲取當前的活動標籤
    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        let activeTab = tabs[0];
        
        // 向背景頁面發送消息，請求翻譯
        chrome.runtime.sendMessage({ action: 'translate', tabId: activeTab.id }, 
		(response) => {
			console.log(response);
			// 在當前頁面執行替換內容的腳本
			chrome.scripting.executeScript({
				target: { tabId: activeTab.id },
				func: (translatedHtml) => {	
					var nodes = document.getElementsByTagName("*");
					for(var i = 0; i < nodes.length; i++) {
						 var el = nodes[i];
						 for (var j = 0; j < el.childNodes.length; j++) {
							 var node = el.childNodes[j];
							 if (node.nodeType === 3 && translatedHtml[node.data] != null){
								 let str = node.data.trim();
								 node.data = translatedHtml[str];
							 }
						 }
					}
				},
				args: [response.translatedTextContent]
			});
        });
    });
});

