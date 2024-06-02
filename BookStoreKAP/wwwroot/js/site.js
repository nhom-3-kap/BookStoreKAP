$(document).ready(function () {
    $(".card-heading[data-url]").each(function (index, item) {
        $(item).on("click", function () {
            const url = $(this).attr("data-url")
            location.href = url
        })
    })
})
