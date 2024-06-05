$(document).ready(function () {
    $(".card-heading[data-url]").each(function (index, item) {
        $(item).on("click", function () {
            const url = $(this).attr("data-url")
            location.href = url
        })
    })
})

    
    src = "https://code.jquery.com/jquery-3.6.0.min.js" 
$(document).ready(function () {
     var rotation = 0;
    $(".side-bar-item").each(function (index,item) {

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

    })
});
                 