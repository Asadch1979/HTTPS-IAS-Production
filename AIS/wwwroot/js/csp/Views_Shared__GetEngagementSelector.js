    function validateEngagementSelection(selectElement) {
        const value = selectElement.value;
        if (!value || value === '0') {
            return false;
        }

        const hasOption = Array.from(selectElement.options)
            .some(option => option.value === value && option.value !== '0');

        if (!hasOption) {
            alert('Select a valid engagement to continue.');
            return false;
        }

        selectElement.form.submit();
        return false;
    }
