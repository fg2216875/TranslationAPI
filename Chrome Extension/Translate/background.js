// chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    // if (request.action === 'translate') {
		// let translateType = request.translateType;
        // // 獲取當前頁面的 HTML 和 Text
        // chrome.scripting.executeScript({
            // target: { tabId: request.tabId },
            // func: () => {
				// //檢查字串是否含有英文大小寫
				// const containsEnglishLetters = (str) => {
					// return /[a-zA-Z]/.test(str);
				// };
				
				// let htmlNodes = document.getElementsByTagName("*");
				// let textNodes = [];
				// let index = 0;
				// for(var i = 0; i < htmlNodes.length; i++) {
					// var el = htmlNodes[i];
					// if(el.nodeName == "SCRIPT" || el.nodeName == "STYLE"){
						// continue;
					// }
					// for (var j = 0; j < el.childNodes.length; j++) {
						// var node = el.childNodes[j];
						// if (node.nodeType === 3 && containsEnglishLetters(node.data.trim())){
							// let keyStr = "{#" + index + "}";
							// textNodes.push({[keyStr]:node.data.trim()});
							// node.data = keyStr;
							// index += 1;
							// continue; 
						// }
					// }
				// }

                // return {
					// textNodes: textNodes
                // };
            // }
        // }, (results) => {
            // let textNodes = results[0].result;

			// // 使用 fetch API 向後端 API 發送請求
			// fetch("https://localhost:7010/api/Translation/translate", {
				// method: 'POST',
				// headers: {
					// 'Content-Type': 'application/json'
				// },
				// body: JSON.stringify({
					// HTMLTextNodes: textNodes,
					// translateType:translateType
				// })
			// }).then(response => {
				// if (!response.ok){
					// console.log("translate fail");
				// }
				// return response.json();
			// })
			// .then(data => {
				// console.log(data);
				// sendResponse(data);
			// })
			// .catch((error) => {
				// console.error('Error:', error);
			// });
        // });

        // // 需要返回 true 以允許異步響應
        // return true;
    // }
// });


chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.action === 'translate') {
        let translateType = request.translateType;

        // 獲取當前頁面的 HTML 和 Text
        chrome.scripting.executeScript({
            target: { tabId: request.tabId },
            func: () => {
                const containsEnglishLetters = (str) => /[a-zA-Z]/.test(str);

                let htmlNodes = document.getElementsByTagName("*");
                let textNodes = [];
                let index = 0;

                for (let i = 0; i < htmlNodes.length; i++) {
                    let el = htmlNodes[i];
                    if (el.nodeName === "SCRIPT" || el.nodeName === "STYLE") {
                        continue;
                    }
                    for (let j = 0; j < el.childNodes.length; j++) {
                        let node = el.childNodes[j];
                        if (node.nodeType === 3 && containsEnglishLetters(node.data.trim())) {
                            let keyStr = "{#" + index + "}";
                            textNodes.push({ [keyStr]: node.data.trim() });
                            node.data = keyStr;
                            index += 1;
                        }
                    }
                }

                return { textNodes };
            }
        }, (results) => {
            let textNodes = results[0].result;

            // 發送 API 請求
            fetch("https://localhost:7010/api/Translation/translate", {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    HTMLTextNodes: textNodes,
                    translateType: translateType
                })
            })
            .then(response => response.json())
            .then(data => {
                console.log("API 回應:", data);

                // 將翻譯後的文字發送到 content.js
                chrome.tabs.sendMessage(request.tabId, {
                    action: "applyTranslation",
                    translatedTextContent: data.translatedTextContent
                });
            })
            .catch(error => console.error('API 錯誤:', error));
        });

        return true; // 允許異步回應
    }
});
