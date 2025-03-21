document.getElementById('replaceButton_Gemini_2.0').addEventListener('click', function() {
	translatePageText("Gemini2.0");
});

document.getElementById('replaceButton_Gemini_2.0_lite').addEventListener('click', function() {
	translatePageText("Gemini2.0-lite");
});

document.getElementById('replaceButton_Gemini_1.5').addEventListener('click', function() {
    translatePageText("Gemini1.5");
});

// document.getElementById('replaceButtonLocal').addEventListener('click', function() {
    // //取得翻譯後的文字並替換
	// translatePageText("Local");
// });


//取得翻譯後的文字並替換
function translatePageText(translateType){
	// 獲取當前的活動標籤
    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        let activeTab = tabs[0];
        
        // 向背景頁面發送消息，請求翻譯
        chrome.runtime.sendMessage({ action: 'translate', tabId: activeTab.id,translateType:translateType }, 
		(response) => {
			console.log(response);
			// 在當前頁面執行替換內容的腳本
			chrome.scripting.executeScript({
				target: { tabId: activeTab.id },
				func: (translatedHtml) => {	
					//將原文替換成翻譯好的文字
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
}

