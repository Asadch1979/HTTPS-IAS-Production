    (function () {
        const form = document.getElementById('finalizeReportForm');
        if (!form) {
            return;
        }

        const finalizeButton = document.getElementById('finalizeReportButton');
        const generateReportButton = document.getElementById('generateReportButton');
        const generateReportHint = document.getElementById('generateReportHint');

        form.addEventListener('submit', async function (event) {
            event.preventDefault();

            if (!confirm('Are you sure you want to finalize this report?')) {
                return;
            }

            const wasDisabledBeforeRequest = finalizeButton.disabled;
            let keepFinalizeDisabled = false;
            finalizeButton.disabled = true;

            try {
                const tokenElement = form.querySelector('input[name="__RequestVerificationToken"]');
                const response = await fetch(form.action, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
                        'RequestVerificationToken': tokenElement ? tokenElement.value : ''
                    },
                    body: new URLSearchParams(new FormData(form)).toString()
                });

                const result = await response.json();
                alert(result.message || 'Unable to finalize report.');

                if (result.success === true) {
                    keepFinalizeDisabled = true;
                    finalizeButton.disabled = true;
                    document.querySelectorAll('.card-body a.btn-primary, .card-body button.btn-primary').forEach(function (item) {
                        item.classList.add('disabled');
                        item.setAttribute('aria-disabled', 'true');
                        item.setAttribute('tabindex', '-1');
                    });

                    if (generateReportButton) {
                        if (generateReportButton.tagName === 'BUTTON') {
                            generateReportButton.disabled = false;
                        }
                        generateReportButton.classList.remove('disabled');
                    }

                    if (generateReportHint) {
                        generateReportHint.textContent = '';
                    }
                }
                else if ((result.message || '').toLowerCase().includes('first complete')) {
                    const checklistCard = document.querySelector('.card.mb-4');
                    if (checklistCard) {
                        checklistCard.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    }
                }
            }
            catch (error) {
                alert('Unable to finalize report. Please try again or contact support.');
            }
            finally {
                if (!keepFinalizeDisabled && !wasDisabledBeforeRequest) {
                    finalizeButton.disabled = false;
                }
            }
        });
    })();
