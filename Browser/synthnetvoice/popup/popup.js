const promptNpcButton = document.getElementById("btn_npc_prompt");
if (promptNpcButton) {
    promptNpcButton.onclick = function () {
        chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
            chrome.tabs.sendMessage(
                tabs[0].id,
                {
                    IsGet: true,
                    IsPost: false,
                    Url: "https://localhost:7230/npc/prompt",
                    Data: {
                        gameName: $("#gameName").val(),
                        npcName: $("#npcName").val(),
                        question: $("#question").val(),
                        scribe: true,
                        gpt: false
                    },
                    PostData: null,
                    ApiCallId: `${guidGenerator()}`,
                    tabId: tabs[0].id
                },
                function (response) {
                    window.close();
                }
            );
            function guidGenerator() {
                const S4 = function () {
                    return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
                };
                return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
            }
        });
    };
}
