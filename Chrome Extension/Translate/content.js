//翻譯結果由 content.js 處理，避免套件被關閉時，還沒把翻譯好的文字替換上去。
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.action === "applyTranslation") {
        let translatedHtml = request.translatedTextContent;

        // 遍歷所有文字節點並替換翻譯內容
        let nodes = document.getElementsByTagName("*");
        for (let i = 0; i < nodes.length; i++) {
            let el = nodes[i];
            for (let j = 0; j < el.childNodes.length; j++) {
                let node = el.childNodes[j];
                if (node.nodeType === 3 && translatedHtml[node.data] != null) {
                    node.data = translatedHtml[node.data];
                }
            }
        }
        console.log("翻譯已應用至當前頁面！");
    }
});
