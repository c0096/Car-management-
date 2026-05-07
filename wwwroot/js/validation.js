document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("form[data-validate='true']").forEach(form => {
        form.addEventListener("submit", event => {
            let valid = true;

            form.querySelectorAll("[data-valmsg-for]").forEach(message => {
                message.textContent = "";
            });

            form.querySelectorAll("input, textarea, select").forEach(field => {
                field.classList.remove("input-validation-error");

                if (field.type === "hidden" || field.type === "file" || field.disabled) {
                    return;
                }

                const requiredMessage = field.getAttribute("data-val-required");
                const maxLength = field.getAttribute("data-val-length-max");
                const maxLengthMessage = field.getAttribute("data-val-length");
                const phoneMessage = field.getAttribute("data-val-phone");
                const value = field.value.trim();
                let message = "";

                if (requiredMessage && value.length === 0) {
                    message = requiredMessage;
                } else if (maxLength && value.length > Number(maxLength)) {
                    message = maxLengthMessage || `Maximum ${maxLength} caractères.`;
                } else if (phoneMessage && value.length > 0 && !/^[0-9+\-\s().]{6,40}$/.test(value)) {
                    message = phoneMessage;
                }

                if (message.length > 0) {
                    valid = false;
                    field.classList.add("input-validation-error");
                    const validationMessage = form.querySelector(`[data-valmsg-for="${field.name}"]`);

                    if (validationMessage) {
                        validationMessage.textContent = message;
                    }
                }
            });

            if (!valid) {
                event.preventDefault();
            }
        });
    });
});
