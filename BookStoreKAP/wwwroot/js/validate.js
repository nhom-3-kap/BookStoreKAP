function Validator(formSelector) {
    let isInvalidData = true;

    function getParent(element, selector) {
        while ($(element).parent().length) {
            if ($(element).parent().is(selector)) {
                return $(element).parent();
            }
            element = $(element).parent();
        }
    }

    const formRules = {};

    const validatorRules = {
        required: function (value, errorMessage = "Vui lòng nhập trường này!") {
            return value ? undefined : errorMessage;
        },
        email: function (value, errorMessage = "Trường này phải là email!") {
            const regexEmail = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;
            return regexEmail.test(value) ? undefined : errorMessage;
        },
        min: function (min) {
            return function (value, errorMessage = `Trường này phải có ít nhất ${min} ký tự!`) {
                return value.length >= min ? undefined : errorMessage;
            };
        },
        max: function (max) {
            return function (value, errorMessage = `Trường này chỉ được phép chứa lớn nhất ${max} ký tự!`) {
                return value.length <= max ? undefined : errorMessage;
            };
        },
        phone: function (value, errorMessage = "Trường này phải là số điện thoại!") {
            const regexPhoneNumber = /^\+?[0-9]{1,3}?[-. ]?([0-9]{2,4}[-. ]?){2,4}[0-9]{1,4}$/;
            return regexPhoneNumber.test(value) ? undefined : errorMessage;
        },
    };

    const formElement = $(formSelector);
    if (formElement.length) {
        const inputs = formElement.find("[name][rules]");

        inputs.each(function () {
            const input = $(this);
            const rules = input.attr("rules").split("|");

            rules.forEach(function (rule) {
                let ruleInfo = "";
                const isRuleHasValue = rule.includes(":");

                if (isRuleHasValue) {
                    ruleInfo = rule.split(":");
                    rule = ruleInfo[0];
                }

                let ruleFunc = validatorRules[rule];

                if (isRuleHasValue) {
                    ruleFunc = ruleFunc(ruleInfo[1]);
                }

                if (Array.isArray(formRules[input.attr("name")])) {
                    formRules[input.attr("name")].push(ruleFunc);
                } else {
                    formRules[input.attr("name")] = [ruleFunc];
                }
            });

            input.on("blur", handleValidate);
            input.on("input", handleClearError);
        });

        function handleValidate(event) {
            const input = $(event.target);
            const rules = formRules[input.attr("name")];
            let errorMessage = "";

            rules.find(function (rule) {
                errorMessage = rule(input.val());
                return errorMessage;
            });

            if (errorMessage) {
                const formGroup = getParent(event.target, ".form-group");
                if (formGroup.length) {
                    const formControl = formGroup.find("input.form-control");

                    if (formControl.length && !formControl.hasClass("border-danger")) {
                        formControl.addClass("border-danger");
                    }
                    const formMessage = formGroup.find(".form-message");
                    if (formMessage.length) {
                        formMessage.text(errorMessage);
                        formMessage.prop("hidden", false);
                    }
                }
            }
            isInvalidData = !!errorMessage;
        }

        function handleClearError(event) {
            const formGroup = getParent(event.target, ".form-group");
            if (formGroup.length) {
                const formControl = formGroup.find("input.form-control");

                if (formControl.length && formControl.hasClass("border-danger")) {
                    formControl.removeClass("border-danger");
                }

                const formMessage = formGroup.find(".form-message");
                if (formMessage.length) {
                    formMessage.text("");
                    formMessage.prop("hidden", true);
                }
            }
        }
    }

    $(formElement).on("submit", function (e) {
        if (isInvalidData) {
            e.preventDefault();
            const inputs = $("[name][rules]");
            $(inputs).each(function () {
                handleValidate({ target: $(this) });
            });
        }
    });
}
