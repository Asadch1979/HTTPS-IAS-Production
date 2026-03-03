    document.addEventListener('DOMContentLoaded', function () {
        var editModal = document.getElementById('editVersionModal');
        editModal.addEventListener('show.bs.modal', function (event) {
            var button = event.relatedTarget;
            document.getElementById('editVersionId').value = button.getAttribute('data-id');
            document.getElementById('editVersionNo').value = button.getAttribute('data-versionno');
            document.getElementById('editReleaseDate').value = button.getAttribute('data-releasedate');
            document.getElementById('editDescription').value = button.getAttribute('data-description');
            document.getElementById('editIsActive').value = button.getAttribute('data-isactive');
        });

        var addForm = document.getElementById('addVersionForm');
        if (addForm) {
            addForm.addEventListener('submit', async function (e) {
                e.preventDefault();

                var formData = new FormData(addForm);
                var payload = {
                    VersionNo: formData.get('VersionNo'),
                    ReleaseDate: formData.get('ReleaseDate'),
                    Description: formData.get('Description')
                };

                var response = await fetchWithPageId(g_asiBaseURL + '/ApiCalls/AddVersionHistory', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    location.reload();
                }
            });
        }

        var editForm = document.getElementById('editVersionForm');
        if (editForm) {
            editForm.addEventListener('submit', async function (e) {
                e.preventDefault();

                var formData = new FormData(editForm);
                var payload = {
                    VersionId: formData.get('VersionId'),
                    VersionNo: formData.get('VersionNo'),
                    ReleaseDate: formData.get('ReleaseDate'),
                    Description: formData.get('Description'),
                    IsActive: formData.get('IsActive')
                };

                var response = await fetchWithPageId(g_asiBaseURL + '/ApiCalls/UpdateVersionHistory', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    location.reload();
                }
            });
        }
    });
