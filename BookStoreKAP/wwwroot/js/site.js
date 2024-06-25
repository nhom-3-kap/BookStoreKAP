var rotation = 0;

const handleAjaxError = (xhr, status, error) => console.error('Error: ' + error.message);
const handleAjax = (url, data, successCallback, options = {}) => {
    const { type, async, ...others } = options;

    $.ajax({
        url: url,
        type: type || 'POST',
        ...others,
        async: async || false,
        data: data,
        success: successCallback,
        error: handleAjaxError
    });
};
function stringFormat(template, ...args) {
    return template.replace(/{(\d+)}/g, function (match, number) {
        return typeof args[number] !== 'undefined'
            ? args[number]
            : match
            ;
    });
}

function getAuthenticated() {
    let isLogin;
    handleAjax("/Auth/IsAuthenticated", {}, (res) => {
        isLogin = res.success;
    }, { type: "GET" });
    return isLogin;
}

function getCountCartAPI() {
    let count = 0;
    handleAjax("/Cart/GetCountCartAPI", {}, (res) => {
        count = res.success ? res.data : 0;
    }, { type: "GET" });
    return count;
}

$(document).ready(function () {
    $(".card-heading[data-url]").each(function (index, item) {
        $(item).on("click", function () {
            const url = $(this).attr("data-url")
            location.href = url
        })
    });

    $(".side-bar-item").each(function (index, item) {
        $(item).on("click", function () {
            if ($(this).find(".wrap-icon").hasClass("rotate")) {
                $(this).find(".wrap-icon").removeClass("rotate")
                $(this).css("height", "38px")

            } else {
                $(this).find(".wrap-icon").addClass("rotate")
                $(this).css("height", `${$(this).find(".side-bar-item-child").height() + 38}px`)
                console.log(`${$(this).find(".side-bar-item-child").height() + 10}px`)
            }
        });
    });

    if (getAuthenticated()) {
        const count = getCountCartAPI();
        $("#spanCartItemCount").text(count);
    }
})