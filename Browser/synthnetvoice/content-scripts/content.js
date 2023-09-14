chrome.runtime.onMessage.addListener(function (request, sender, sendResponse) {
    if (request.IsGet) {
        // GET request
        $.ajax({
            url: request.Url,
            data: request.Data,
            type: "GET",
            dataType: "json",
            success: function (response) {
                console.log(response);
            },
            error: function (xhr, status, error) {
                console.log("Error: " + error);
            }
        });
    }

    if (request.IsPost) {
        // POST request
        $.ajax({
            url: request.Url,
            type: "POST",
            dataType: "json",
            data: request.PostData,
            success: function (response) {
                console.log(response);
            },
            error: function (xhr, status, error) {
                console.log("Error: " + error);
            }
        });
    }
    sendResponse({ fromcontent: "This message is from content.js" });
});