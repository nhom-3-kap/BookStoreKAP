document.addEventListener('DOMContentLoaded', function () {
    var stars = document.querySelectorAll('.rating-star');
    var ratingStarsContainer = document.getElementById('rating-stars');

    // Hover effect for stars
    stars.forEach(function (star, index) {
        star.addEventListener('mouseenter', function () {
            highlightStars(index);
        });

        star.addEventListener('mouseleave', function () {
            resetStars();
        });

        star.addEventListener('click', function () {
            chooseStars(index);
        });
    });

    ratingStarsContainer.addEventListener('mouseleave', function () {
        resetStars();
    });

    function highlightStars(index) {
        for (var i = 0; i <= index; i++) {
            stars[i].classList.add('rating-hover');
        }
        for (var i = index + 1; i < stars.length; i++) {
            stars[i].classList.remove('rating-hover');
        }
    }

    function resetStars() {
        stars.forEach(function (star) {
            star.classList.remove('rating-hover');
        });
    }

    function chooseStars(index) {
        for (var i = 0; i <= index; i++) {
            stars[i].classList.add('rating-chosen');
        }
        for (var i = index + 1; i < stars.length; i++) {
            stars[i].classList.remove('rating-chosen');
        }
    }
});
