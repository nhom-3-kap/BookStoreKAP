function initializeOwlCarousel(selector, options) {
    if ($(selector).length > 1) {
        $(selector).each(function (index) {
            const that = this;
            $(this).owlCarousel({
                autoplay: options.autoplay || true,
                loop: options.loop || true,
                margin: options.margin || 10,
                nav: options.nav || true,
                responsive: options.responsive || {
                    0: {
                        items: 1
                    },
                    600: {
                        items: 3
                    },
                    1000: {
                        items: 5
                    }

                }
            });
    
            if (options.controlLeft) {
                $(`#kap-carousel-newreleases-${index+1} ~ #carousel-controls-${index+1} ${options.controlLeft}`).on("click", function () {
                    $(that).trigger('prev.owl.carousel');
             

                });
            }

            if (options.controlRight) {
                $(`#carousel-controls-${index + 1} ${options.controlRight}`).on("click", function () {
                    $(that).trigger('next.owl.carousel');
                });
            }

        })

    } else {
        $(selector).owlCarousel({
        autoplay: options.autoplay || true,
        loop: options.loop || true,
        margin: options.margin || 10,
        nav: options.nav || true,
        responsive: options.responsive || {
            0: {
                items: 1
            },
            600: {
                items: 3
            },
            1000: {
                items: 5
            }
        }
    });

    if (options.controlLeft) {
        $(`${selector} ~ .carousel-controls ${options.controlLeft}`).on("click", function () {
            $(selector).trigger('prev.owl.carousel');

        });
    }

    if (options.controlRight) {
        $(`${selector} ~ .carousel-controls ${options.controlRight}`).on("click", function () {
            $(selector).trigger('next.owl.carousel');
        });
    }
    }
    
}

$(document).ready(function () {
    initializeOwlCarousel(".kap-carousel-newreleases", {
        controlLeft: '.control-left',
        controlRight: '.control-right'
    });

    initializeOwlCarousel("#carousel-bestsellers", {
        controlLeft: '.control-left',
        controlRight: '.control-right'
    });

    initializeOwlCarousel("#carousel-feature-book", {
        controlLeft: '.control-left',
        controlRight: '.control-right'
    });

    initializeOwlCarousel("#slider-content", {
        responsive: {
            0: {
                items: 1
            },
            600: {
                items: 1
            },
            1000: {
                items: 1
            }
        }
    });

    initializeOwlCarousel("#carousel-sold-book", {
        controlLeft: '.control-left',
        controlRight: '.control-right'
    });

    initializeOwlCarousel("#carousel-feature-author", {
        controlLeft: '.control-left',
        controlRight: '.control-right',
        responsive: {
            0: {
                items: 3
            },
            600: {
                items: 5
            },
            1000: {
                items: 7
            }
        }
    });
});
