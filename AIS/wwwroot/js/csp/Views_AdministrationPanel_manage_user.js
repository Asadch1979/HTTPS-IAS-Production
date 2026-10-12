var g_userList = [];
var g_removingUserContext = false;
var g_userId = 0;
var g_userContexts = [];
var g_editingContextIndex = null;

$(document).ready(function () {
    if ($('#roleSaveField').length && $.fn.select2) {
        $('#roleSaveField').select2();
        $('#roleSaveField').css('width', '100%');
    }

    initializeEntitySelection();
    $('#isActiveSaveField').prop('checked', true);
    $(document).off('click.manageUsers change.manageUsers');
    $(document).on('click.manageUsers', '#addAssignmentButton', assignmentActionButtonClickHandler);
    $(document).on('click.manageUsers', '.js-edit-context', editUserContextClickHandler);
    $(document).on('click.manageUsers', '.js-remove-context', removeUserContextClickHandler);
    $(document).on('change.manageUsers', '.js-default-context', defaultContextChangeHandler);
    $(document).on('change.manageUsers', '.js-context-active', contextActiveChangeHandler);
});

function normalizeFlag(value, defaultValue) {
    var flag = (value || defaultValue || 'N').toString().trim().toUpperCase();
    return flag === 'Y' ? 'Y' : 'N';
}

function normalizeOptionalText(value) {
    var text = (value || '').toString().trim();
    return text === '' ? null : text;
}

function normalizeOptionalDateValue(value) {
    var text = normalizeOptionalText(value);
    return text || null;
}

function escapeHtml(value) {
    return $('<div>').text(value || '').html();
}

function getOptionText(selector) {
    return ($(selector).text() || '').trim();
}

function toInt(value) {
    var parsed = parseInt(value, 10);
    return isNaN(parsed) ? 0 : parsed;
}

function uniqueJoin(values) {
    var seen = {};
    var result = [];
    $.each(values || [], function (_, value) {
        var text = (value || '').toString().trim();
        var key = text.toLowerCase();
        if (!text || seen[key]) {
            return;
        }

        seen[key] = true;
        result.push(text);
    });

    return result.join(', ');
}

function mapApiContext(context) {
    if (!context) {
        return null;
    }

    return {
        assignmentId: toInt(context.assignmentId || context.userContextId || context.userContextAssignmentId || context.ASSIGNMENT_ID || context.USER_CONTEXT_ID),
        roleId: toInt(context.roleId || context.groupId || context.ROLE_ID || context.GROUP_ID),
        groupId: toInt(context.groupId || context.roleId || context.GROUP_ID || context.ROLE_ID),
        roleName: (context.roleName || context.userRoleName || context.GROUP_NAME || '').toString(),
        entityId: toInt(context.entityId || context.ENTITY_ID),
        entityName: (context.entityName || context.userEntityName || context.ENT_NAME || '').toString(),
        parentEntityId: toInt(context.parentEntityId || context.PARENT_ENTITY_ID),
        parentEntityName: (context.parentEntityName || context.userParentEntityName || context.PARENT_ENTITY_NAME || '').toString(),
        entityTypeId: toInt(context.entityTypeId || context.userEntityTypeID || context.UserEntityTypeID || context.C_TYPE_ID || context.c_TYPE_ID || context.ENTITY_TYPE_ID),
        entityTypeName: (context.entityTypeName || '').toString(),
        relationshipTypeId: toInt(context.relationshipTypeId || context.RELATIONSHIP_TYPE_ID),
        relationshipTypeName: (context.relationshipTypeName || context.RELATIONSHIP_TYPE_NAME || '').toString(),
        isDefault: normalizeFlag(context.isDefault || context.IS_DEFAULT || context.ISDEFAULT, 'N'),
        isActive: normalizeFlag(context.isActive || context.IS_ACTIVE || context.ISACTIVE, 'Y'),
        assignmentType: normalizeOptionalText(context.assignmentType || context.ASSIGNMENT_TYPE) || 'MANUAL',
        effectiveFrom: normalizeOptionalDateValue(context.effectiveFrom || context.EFFECTIVE_FROM),
        effectiveTo: normalizeOptionalDateValue(context.effectiveTo || context.EFFECTIVE_TO),
        remarks: normalizeOptionalText(context.remarks || context.REMARKS),
        isDeleted: !!(context.isDeleted || context.ISDELETED)
    };
}

function buildLegacyContextFromUser(user) {
    return {
        assignmentId: 0,
        roleId: toInt(user.userRoleID || user.userGroupID),
        groupId: toInt(user.userGroupID || user.userRoleID),
        roleName: (user.userRole || user.userGroup || '').toString(),
        entityId: toInt(user.userEntityID),
        entityName: (user.userEntityName || '').toString(),
        parentEntityId: toInt(user.userParentEntityID),
        parentEntityName: (user.userParentEntityName || '').toString(),
        entityTypeId: toInt(user.userEntityTypeID || user.UserEntityTypeID || user.C_TYPE_ID || user.c_TYPE_ID),
        entityTypeName: '',
        relationshipTypeId: toInt(user.relationshipId),
        relationshipTypeName: '',
        isDefault: 'Y',
        isActive: normalizeFlag(user.isActive, 'Y'),
        assignmentType: 'MANUAL',
        effectiveFrom: null,
        effectiveTo: null,
        remarks: null,
        isDeleted: false
    };
}

function mergeUserRows(users) {
    var grouped = {};

    $.each(users || [], function (_, user) {
        var key = (user.id || 0) > 0 ? user.id.toString() : (user.ppNumber || ('row_' + Math.random()));
        if (!grouped[key]) {
            grouped[key] = $.extend({}, user);
            grouped[key]._roles = [];
            grouped[key]._entities = [];
            grouped[key]._parents = [];
            grouped[key]._contextKeys = {};
        }

        var current = grouped[key];
        current._roles.push(user.userRole);
        current._entities.push(user.userEntityName);
        current._parents.push(user.userParentEntityName);

        var contextKey = [
            toInt(user.userRoleID || user.userGroupID),
            toInt(user.userEntityID),
            toInt(user.relationshipId),
            toInt(user.userParentEntityID)
        ].join(':');
        current._contextKeys[contextKey] = true;
    });

    return $.map(grouped, function (user) {
        user.userRole = uniqueJoin(user._roles) || user.userRole;
        user.userGroup = user.userRole;
        user.userEntityName = uniqueJoin(user._entities) || user.userEntityName;
        user.userParentEntityName = uniqueJoin(user._parents) || user.userParentEntityName;
        user.assignmentCount = Math.max(Object.keys(user._contextKeys || {}).length, user.assignmentCount || 1);
        delete user._roles;
        delete user._entities;
        delete user._parents;
        delete user._contextKeys;
        return user;
    });
}

function renderUserList(users) {
    var $tbody = $('#userListTable tbody');
    $tbody.empty();

    if (!users || users.length === 0) {
        $tbody.append('<tr><td colspan="9" class="text-center text-muted">No users matched the provided criteria.</td></tr>');
        return;
    }

    $.each(users, function (_, user) {
        $tbody.append(
            '<tr id="userrecordrow_' + user.id + '">' +
            '<td>' + escapeHtml(user.name) + '</td>' +
            '<td>' + escapeHtml(user.ppNumber) + '</td>' +
            '<td>' + escapeHtml(user.userParentEntityName) + '</td>' +
            '<td>' + escapeHtml(user.userEntityName) + '</td>' +
            '<td>' + escapeHtml(user.email) + '</td>' +
            '<td>' + escapeHtml(user.userRole) + '</td>' +
            '<td>' + escapeHtml((user.assignmentCount || 1).toString()) + '</td>' +
            '<td>' + escapeHtml(user.isActive) + '</td>' +
            '<td><a class="text-danger editmode" href="#" data-onclick="event.preventDefault();editUserDetails(' + user.id + ')">Edit</a><a class="text-danger cancelmode d-none" href="#" data-onclick="cancelEditUserDetails(' + user.id + ')">Cancel</a></td>' +
            '</tr>'
        );
    });
}

function normalizeContextCollection(preferredDefaultIndex) {
    $.each(g_userContexts, function (index, context) {
        if (!context) {
            return;
        }

        context.assignmentId = toInt(context.assignmentId);
        context.roleId = toInt(context.roleId);
        context.groupId = toInt(context.groupId || context.roleId);
        context.entityId = toInt(context.entityId);
        context.parentEntityId = toInt(context.parentEntityId);
        context.relationshipTypeId = toInt(context.relationshipTypeId);
        context.isDeleted = !!context.isDeleted;
        context.isDefault = context.isDeleted ? 'N' : normalizeFlag(context.isDefault, 'N');
        context.isActive = context.isDeleted ? 'N' : normalizeFlag(context.isActive, 'Y');
        context.assignmentType = normalizeOptionalText(context.assignmentType) || 'MANUAL';
        context.effectiveFrom = normalizeOptionalDateValue(context.effectiveFrom);
        context.effectiveTo = normalizeOptionalDateValue(context.effectiveTo);
        context.remarks = normalizeOptionalText(context.remarks);
    });

    if (typeof preferredDefaultIndex === 'number' && preferredDefaultIndex >= 0 && g_userContexts[preferredDefaultIndex] && !g_userContexts[preferredDefaultIndex].isDeleted) {
        $.each(g_userContexts, function (index, context) {
            if (!context || context.isDeleted) {
                return;
            }

            context.isDefault = index === preferredDefaultIndex ? 'Y' : 'N';
        });
    }
}

function renderUserContexts() {
    var $tbody = $('#userContextTableBody');
    $tbody.empty();

    var hasVisibleContexts = false;
    $.each(g_userContexts, function (index, context) {
        if (!context || context.isDeleted) {
            return;
        }

        hasVisibleContexts = true;
        var isDefaultChecked = context.isDefault === 'Y' ? 'checked="checked"' : '';
        var isActiveChecked = context.isActive === 'Y' ? 'checked="checked"' : '';

        $tbody.append(
            '<tr data-context-index="' + index + '">' +
            '<td class="text-center"><input type="radio" class="js-default-context" name="defaultUserContext" data-context-index="' + index + '" ' + isDefaultChecked + ' /></td>' +
            '<td class="text-center"><input type="checkbox" class="js-context-active" data-context-index="' + index + '" ' + isActiveChecked + ' /></td>' +
            '<td>' + escapeHtml(context.roleName) + '</td>' +
            '<td>' + escapeHtml(context.parentEntityName) + '</td>' +
            '<td>' + escapeHtml(context.entityName) + '</td>' +
            '<td>' + escapeHtml(context.entityTypeName || entityTypeLabel(context.entityTypeId) || 'Unresolved') + '</td>' +
            '<td class="text-center"><a href="#" class="js-edit-context" data-context-index="' + index + '">Edit</a> | <a href="#" class="js-remove-context" data-context-index="' + index + '">Remove</a></td>' +
            '</tr>'
        );
    });

    if (!hasVisibleContexts) {
        $tbody.append('<tr><td colspan="7" class="text-center text-muted">No role and posting assignments added yet.</td></tr>');
    }
}

function resetUserContexts() {
    g_userContexts = [];
    resetAssignmentEditMode();
    renderUserContexts();
}

function findContextIndex(candidate, includeDeleted) {
    var matchIndex = -1;

    $.each(g_userContexts, function (index, context) {
        if (!context || (!includeDeleted && context.isDeleted)) {
            return;
        }

        if (candidate.assignmentId > 0 && context.assignmentId > 0 && candidate.assignmentId === context.assignmentId) {
            matchIndex = index;
            return false;
        }

        if (context.roleId === candidate.roleId &&
            context.entityId === candidate.entityId &&
            context.groupId === candidate.groupId) {
            matchIndex = index;
            return false;
        }
    });

    return matchIndex;
}

function buildCurrentContext() {
    var roleId = toInt($('#roleSaveField option:selected').val());
    var entityId = toInt($('#childposting option:selected').val());
    if (roleId <= 0 || entityId <= 0 || !g_selectionReady) {
        return null;
    }

    return {
        assignmentId: 0,
        roleId: roleId,
        groupId: roleId,
        roleName: getOptionText('#roleSaveField option:selected'),
        entityId: entityId,
        entityName: getOptionText('#childposting option:selected'),
        parentEntityId: toInt($('#controlingsearch option:selected').val()),
        parentEntityName: getOptionText('#controlingsearch option:selected'),
        entityTypeId: toInt($('#EntityTypeField').val()),
        entityTypeName: getOptionText('#EntityTypeField option:selected'),
        relationshipTypeId: 0,
        relationshipTypeName: '',
        isDefault: $('#defaultAssignmentSaveField').is(':checked') ? 'Y' : 'N',
        isActive: $('#isActiveSaveField').is(':checked') ? 'Y' : 'N',
        assignmentType: 'MANUAL',
        effectiveFrom: null,
        effectiveTo: null,
        remarks: null,
        isDeleted: false
    };
}

function upsertContext(candidate) {
    var existingIndex = findContextIndex(candidate, false);
    if (existingIndex < 0) {
        existingIndex = findContextIndex(candidate, true);
    }

    if (existingIndex >= 0) {
        var existing = g_userContexts[existingIndex];
        candidate = $.extend({}, existing, candidate, { relationshipTypeId: existing.relationshipTypeId, relationshipTypeName: existing.relationshipTypeName, assignmentType: existing.assignmentType, effectiveFrom: existing.effectiveFrom, effectiveTo: existing.effectiveTo, remarks: existing.remarks });
        candidate.assignmentId = existing.assignmentId || candidate.assignmentId;
        candidate.isDeleted = false;
        g_userContexts[existingIndex] = candidate;
    } else {
        g_userContexts.push(candidate);
        existingIndex = g_userContexts.length - 1;
    }

    normalizeContextCollection(candidate.isDefault === 'Y' ? existingIndex : null);
    renderUserContexts();
}

function resetAssignmentEditMode() {
    g_editingContextIndex = null;
    $('#addAssignmentButton').text('Add Assignment');
}

function assignmentActionButtonClickHandler(event) {
    event.preventDefault();

    if (typeof g_editingContextIndex === 'number') {
        updateAssignmentButtonClickHandler();
        return;
    }

    addAssignmentButtonClickHandler();
}

function addAssignmentButtonClickHandler() {
    var candidate = buildCurrentContext();
    if (!candidate) {
        alert('Select one role and one place of posting before adding an assignment.');
        return;
    }

    upsertContext(candidate);
}

function editUserContextClickHandler(event) {
    event.preventDefault();

    var index = toInt($(this).attr('data-context-index'));
    if (!g_userContexts[index] || g_userContexts[index].isDeleted) {
        return;
    }

    g_editingContextIndex = index;
    applyContextToBuilder(g_userContexts[index]);
    $('#addAssignmentButton').text('Update Assignment');
}

function updateAssignmentButtonClickHandler() {
    var index = g_editingContextIndex;
    if (typeof index !== 'number' || !g_userContexts[index] || g_userContexts[index].isDeleted) {
        resetAssignmentEditMode();
        return;
    }

    var candidate = buildCurrentContext();
    if (!candidate) {
        alert('Select one role and one place of posting before updating the assignment.');
        return;
    }

    var duplicateIndex = findContextIndex(candidate, false);
    if (duplicateIndex >= 0 && duplicateIndex !== index) {
        alert('Duplicate role and posting assignments are not allowed.');
        return;
    }

    candidate = $.extend({}, g_userContexts[index], candidate, {
        relationshipTypeId: g_userContexts[index].relationshipTypeId,
        relationshipTypeName: g_userContexts[index].relationshipTypeName,
        assignmentType: g_userContexts[index].assignmentType,
        effectiveFrom: g_userContexts[index].effectiveFrom,
        effectiveTo: g_userContexts[index].effectiveTo,
        remarks: g_userContexts[index].remarks
    });
    candidate.assignmentId = g_userContexts[index].assignmentId || 0;
    candidate.isDeleted = false;
    g_userContexts[index] = candidate;

    normalizeContextCollection(candidate.isDefault === 'Y' ? index : null);
    renderUserContexts();
    resetAssignmentEditMode();
}

function removeUserContextClickHandler(event) {
    event.preventDefault();
    removeUserContext(toInt($(this).attr('data-context-index')));
}

function removeUserContext(index) {
    var context = g_userContexts[index];
    if (!context || g_removingUserContext) {
        return;
    }
    var savedAssignment = context.assignmentId > 0;
    var confirmation = savedAssignment
        ? 'Remove this assignment immediately? If this is the final assignment, the user will lose IAS access. This does not require Save.'
        : 'Remove this unsaved assignment?';
    if (!window.confirm(confirmation)) {
        return;
    }
    if (!savedAssignment) {
        g_userContexts.splice(index, 1);
        normalizeContextCollection();
        renderUserContexts();
        resetAssignmentEditMode();
        return;
    }

    var userId = g_userId;
    g_removingUserContext = true;
    $('#saveChangesButton, #addNewUserChangesButton').prop('disabled', true);
    $.ajax({
        url: g_asiBaseURL + '/AdministrationPanel/delete_user_context_assignment',
        type: 'POST',
        data: { userId: userId, assignmentId: context.assignmentId },
        cache: false,
        dataType: 'json',
        success: function (data) {
            if (!data || !data.status) {
                showApiAlert(data, 'Unable to remove assignment.');
                return;
            }
            // Refresh persisted rows/default flags without sending pending edits to Save.
            if (g_userId === userId) {
                g_userContexts = $.map(data.assignments || [], mapApiContext);
                normalizeContextCollection();
                renderUserContexts();
                resetAssignmentEditMode();
                applyContextToBuilder(g_userContexts.filter(function (item) { return item.isDefault === 'Y'; })[0] || g_userContexts[0]);
                if (data.userRemoved) {
                    cancelEditUserDetails(userId);
                }
            }
            if (!data.userRemoved) {
                $.each(g_userList, function (_, user) {
                    if (user.id !== userId) { return; }
                    var contexts = $.map(data.assignments || [], mapApiContext);
                    user.assignmentCount = contexts.length;
                    user.userRole = uniqueJoin(contexts.map(function (item) { return item.roleName; }));
                    user.userEntityName = uniqueJoin(contexts.map(function (item) { return item.entityName; }));
                    user.userParentEntityName = uniqueJoin(contexts.map(function (item) { return item.parentEntityName; }));
                });
                renderUserList(g_userList);
                if (g_userId === userId) {
                    $('#userrecordrow_' + userId + ' .editmode').addClass('d-none');
                    $('#userrecordrow_' + userId + ' .cancelmode').removeClass('d-none');
                }
            }
            if (data.userRemoved) {
                g_userList = g_userList.filter(function (user) { return user.id !== userId; });
                renderUserList(g_userList);
            }
            showApiAlert(data, 'Assignment removed successfully.');
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), 'Unable to remove assignment.');
        },
        complete: function () {
            g_removingUserContext = false;
            $('#saveChangesButton, #addNewUserChangesButton').prop('disabled', false);
        }
    });
}

function defaultContextChangeHandler() {
    setDefaultContext(toInt($(this).attr('data-context-index')));
}

function setDefaultContext(index) {
    normalizeContextCollection(index);
    renderUserContexts();
}

function contextActiveChangeHandler() {
    toggleContextActive(toInt($(this).attr('data-context-index')), this.checked);
}

function toggleContextActive(index, isActive) {
    if (!g_userContexts[index]) {
        return;
    }

    g_userContexts[index].isActive = isActive ? 'Y' : 'N';
    if (!isActive && g_userContexts[index].isDefault === 'Y') {
        g_userContexts[index].isDefault = 'N';
    }

    normalizeContextCollection();
    renderUserContexts();
}

function applyContextToBuilder(context) {
    if (!context) {
        return;
    }

    $('#roleSaveField').val(context.groupId || context.roleId).trigger('change');
    $('#defaultAssignmentSaveField').prop('checked', context.isDefault === 'Y');
    $('#isActiveSaveField').prop('checked', context.isActive === 'Y');
    restoreEntitySelection(context);
}

function loadUserContexts(user) {
    var requestedUserId = g_userId;
    if (!user || !user.id) {
        resetUserContexts();
        return;
    }

    $.ajax({
        url: g_asiBaseURL + '/ApiCalls/GetUserContexts',
        type: 'POST',
        data: {
            userId: user.id,
            ppNumber: user.ppNumber
        },
        cache: false,
        success: function (data) {
            if (requestedUserId !== g_userId) return;
            g_userContexts = $.map(data || [], function (context) {
                return mapApiContext(context);
            });

            if (!g_userContexts.length) {
                g_userContexts = [buildLegacyContextFromUser(user)];
            }

            normalizeContextCollection();
            renderUserContexts();
            var contextsToResolve = g_userContexts;
            (async function () {
                for (var i = 0; i < contextsToResolve.length; i++) {
                    if (requestedUserId !== g_userId || contextsToResolve !== g_userContexts) return;
                    try { await resolveContextEntityType(contextsToResolve[i]); } catch (_) { /* Preserve unresolved assignments. */ }
                }
                if (requestedUserId === g_userId && contextsToResolve === g_userContexts) renderUserContexts();
            })();

            var defaultContext = null;
            $.each(g_userContexts, function (_, context) {
                if (context && !context.isDeleted && context.isDefault === 'Y') {
                    defaultContext = context;
                    return false;
                }
            });

            applyContextToBuilder(defaultContext || g_userContexts[0]);
        },
        error: function (xhr) {
            if (requestedUserId !== g_userId) return;
            resetUserContexts();
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), 'Unable to load user assignments.');
        },
        dataType: 'json'
    });
}

function findButtonClickHandler() {
    var whereCheck = 0;
    var entityId = 0;

    if ($('#ppnoSearchField').val() !== '') {
        whereCheck++;
    }
    if ($('#loginSearchField').val() !== '') {
        whereCheck++;
    }
    if (toInt($('#childposting').val()) > 0) {
        whereCheck++;
        entityId = $('#childposting').val();
    } else if (toInt($('#controlingsearch').val()) > 0) {
        whereCheck++;
        entityId = $('#controlingsearch').val();
    }
    if ($('#emailSearchField').val() !== '') {
        whereCheck++;
    }
    if ($('#roleSearchField option:selected').val() !== '0') {
        whereCheck++;
    }

    if (whereCheck === 0) {
        alert('Please provide at least one filter to proceed');
        return;
    }

    $.ajax({
        url: g_asiBaseURL + '/ApiCalls/FindUsers',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            ppNumber: $('#ppnoSearchField').val(),
            loginName: $('#loginSearchField').val(),
            entityId: entityId,
            email: $('#emailSearchField').val(),
            groupId: $('#roleSearchField option:selected').val()
        }),
        cache: false,
        success: function (data) {
            g_userList = mergeUserRows(data || []);
            renderUserList(g_userList);
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), 'Unable to search users.');
        },
        dataType: 'json'
    });
}

function editUserDetails(userId) {
    g_userId = userId;
    $('#userrecordrow_' + userId).css({ background: 'rgba(0,0,0,.05)', 'font-weight': 'bold', color: 'navy' });
    $('#userrecordrow_' + userId + ' .editmode').addClass('d-none');
    $('#userrecordrow_' + userId + ' .cancelmode').removeClass('d-none');
    $('#saveChangesButton').removeClass('d-none');
    $('#addNewUserChangesButton').removeClass('d-none');
    $('#findButton').addClass('d-none');
    $('#searchFieldsContainer').addClass('d-none');
    $('#saveChangesFieldsContainer').removeClass('d-none');

    $.each(g_userList, function (_, user) {
        if (user.id !== userId) {
            return;
        }

        $('#ppnoSaveField').val(user.ppNumber);
        $('#loginSaveField').val(user.ppNumber);
        $('#emailSaveField').val(user.email);
        $('#isActiveSaveField').prop('checked', normalizeFlag(user.isActive, 'Y') === 'Y');
        $('#roleSaveField').val(user.userGroupID || user.userRoleID || 0).trigger('change');

        loadUserContexts(user);
    });
}

function cancelEditUserDetails(userId) {
    g_userId = 0;
    $('#userrecordrow_' + userId).css({ background: 'none', 'font-weight': 'normal', color: 'black' });
    $('#userrecordrow_' + userId + ' .editmode').removeClass('d-none');
    $('#userrecordrow_' + userId + ' .cancelmode').addClass('d-none');
    $('#saveChangesButton').addClass('d-none');
    $('#addNewUserChangesButton').addClass('d-none');
    $('#findButton').removeClass('d-none');
    $('#saveChangesFieldsContainer').addClass('d-none');
    $('#searchFieldsContainer').removeClass('d-none');
    clearSearchFields();
}

function clearSearchFields() {
    $('#ppnoSaveField').val('');
    $('#loginSaveField').val('');
    $('#emailSaveField').val('');
    $('#passSaveField').val('');
    $('#conPassSaveField').val('');
    $('#roleSaveField').val(0).trigger('change');
    ++g_selectionVersion;
    g_selectionReady = false;
    $('#EntityTypeField').val(0);
    selectionStatus('');
    $('#controlingsearch').empty().append('<option id="0" value="0">--Select Controlling/Reporting Office--</option>');
    $('#childposting').empty().append('<option id="0" value="0">--Select Place of Posting--</option>');
    $('#isActiveSaveField').prop('checked', true);
    $('#defaultAssignmentSaveField').prop('checked', false);
    $('#resetpassword').prop('checked', false);
    resetUserContexts();
}

function buildAssignmentPayloadFromContext(context, isDeleted) {
    return {
        ASSIGNMENT_ID: context.assignmentId > 0 ? context.assignmentId : null,
        USER_CONTEXT_ID: context.assignmentId > 0 ? context.assignmentId : null,
        ROLE_ID: context.roleId > 0 ? context.roleId : null,
        GROUP_ID: context.groupId > 0 ? context.groupId : (context.roleId > 0 ? context.roleId : null),
        ENTITY_ID: context.entityId > 0 ? context.entityId : null,
        ISDEFAULT: isDeleted ? 'N' : normalizeFlag(context.isDefault, 'N'),
        ISACTIVE: isDeleted ? 'N' : normalizeFlag(context.isActive, 'Y'),
        ASSIGNMENT_TYPE: normalizeOptionalText(context.assignmentType) || 'MANUAL',
        EFFECTIVE_FROM: normalizeOptionalDateValue(context.effectiveFrom),
        EFFECTIVE_TO: normalizeOptionalDateValue(context.effectiveTo),
        REMARKS: normalizeOptionalText(context.remarks),
        ISDELETED: !!isDeleted
    };
}

function collectAssignmentsFromGrid() {
    var visibleAssignments = [];
    var deletedAssignments = [];
    var rowCount = 0;

    $('#userContextTableBody tr[data-context-index]').each(function () {
        var contextIndex = toInt($(this).attr('data-context-index'));
        var context = g_userContexts[contextIndex];
        if (!context || context.isDeleted) {
            return;
        }

        rowCount++;
        visibleAssignments.push(buildAssignmentPayloadFromContext(context, false));
    });

    $.each(g_userContexts, function (_, context) {
        if (!context || !context.isDeleted || !(context.assignmentId > 0)) {
            return;
        }

        deletedAssignments.push(buildAssignmentPayloadFromContext(context, true));
    });

    return {
        rowCount: rowCount,
        visibleAssignments: visibleAssignments,
        assignments: deletedAssignments.concat(visibleAssignments)
    };
}

function validateAssignments(collectedAssignments) {
    if (!collectedAssignments || collectedAssignments.rowCount === 0 || !collectedAssignments.visibleAssignments.length) {
        return 'Add at least one role and posting assignment to the grid before saving the user.';
    }

    var duplicateRows = {};
    var activeCount = 0;
    var defaultRows = [];
    var inactiveDefaultRow = 0;

    $.each(collectedAssignments.visibleAssignments, function (index, assignment) {
        var rowNumber = index + 1;

        if (!(assignment.ROLE_ID > 0) || !(assignment.ENTITY_ID > 0)) {
            duplicateRows.invalid = rowNumber;
            return false;
        }

        assignment.GROUP_ID = assignment.GROUP_ID || assignment.ROLE_ID;
        assignment.ISDEFAULT = normalizeFlag(assignment.ISDEFAULT, 'N');
        assignment.ISACTIVE = normalizeFlag(assignment.ISACTIVE, 'Y');
        assignment.ASSIGNMENT_TYPE = normalizeOptionalText(assignment.ASSIGNMENT_TYPE) || 'MANUAL';
        assignment.EFFECTIVE_FROM = normalizeOptionalDateValue(assignment.EFFECTIVE_FROM);
        assignment.EFFECTIVE_TO = normalizeOptionalDateValue(assignment.EFFECTIVE_TO);
        assignment.REMARKS = normalizeOptionalText(assignment.REMARKS);

        if (assignment.ISACTIVE === 'Y') {
            activeCount++;
        }

        if (assignment.ISDEFAULT === 'Y') {
            defaultRows.push(rowNumber);
            if (assignment.ISACTIVE !== 'Y' && !inactiveDefaultRow) {
                inactiveDefaultRow = rowNumber;
            }
        }

        var duplicateKey = [assignment.ROLE_ID, assignment.GROUP_ID, assignment.ENTITY_ID].join(':');
        if (duplicateRows[duplicateKey]) {
            duplicateRows.duplicate = duplicateRows[duplicateKey] + ',' + rowNumber;
            return false;
        }

        duplicateRows[duplicateKey] = rowNumber;
        return true;
    });

    if (duplicateRows.invalid) {
        return 'Assignment row ' + duplicateRows.invalid + ' must include one role and one entity.';
    }

    if (duplicateRows.duplicate) {
        var rows = duplicateRows.duplicate.split(',').join(' and ');
        return 'Duplicate role and posting assignments are not allowed. Check rows ' + rows + '.';
    }

    if (activeCount === 0) {
        return 'At least one active role and posting assignment is required.';
    }

    if (defaultRows.length === 0) {
        return 'Mark one active assignment as the default before saving.';
    }

    if (defaultRows.length > 1) {
        return 'Only one assignment can be marked as default. Check rows ' + defaultRows.join(', ') + '.';
    }

    if (inactiveDefaultRow) {
        return 'Default assignment row ' + inactiveDefaultRow + ' must be active.';
    }

    return null;
}

function buildSavePayload(isNewUser) {
    if ($('#passSaveField').val() !== $('#conPassSaveField').val()) {
        alert('Password and Confirm Password should be Same');
        return null;
    }

    var collectedAssignments = collectAssignmentsFromGrid();
    var validationMessage = validateAssignments(collectedAssignments);
    if (validationMessage) {
        alert(validationMessage);
        return null;
    }

    if (window.console && console.info) {
        console.info('Manage_user assignments payload', collectedAssignments.assignments);
    }

    return {
        USER_ID: isNewUser ? null : g_userId,
        PPNO: $.trim($('#ppnoSaveField').val()),
        PASSWORD: $('#passSaveField').val(),
        EMAIL_ADDRESS: $.trim($('#emailSaveField').val()),
        ISACTIVE: $('#isActiveSaveField').is(':checked') ? 'Y' : 'N',
        ASSIGNMENTS: collectedAssignments.assignments
    };
}

function persistUserContexts(payload, successMessage) {
    $.ajax({
        url: g_asiBaseURL + '/AdministrationPanel/save_user_contexts',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        cache: false,
        success: function (data) {
            showApiAlert(data, successMessage);
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), successMessage);
        },
        dataType: 'json'
    });
}

function saveButtonClickHandler() {
    var payload = buildSavePayload(false);
    if (!payload) {
        return;
    }

    persistUserContexts(payload, 'User Successfully Updated');
}

function addNewUserButtonClickHandler() {
    var payload = buildSavePayload(true);
    if (!payload) {
        return;
    }

    persistUserContexts(payload, 'User Successfully Created');
}

function checkBoxClicked() {
    if ($('#resetpassword').prop('checked')) {
        $('#passSaveField').val('Ztbl@1234');
        $('#conPassSaveField').val('Ztbl@1234');
    } else {
        $('#passSaveField').val('');
        $('#conPassSaveField').val('');
    }
}
var g_selectionVersion = (window.g_selectionVersion || 0) + 1;
var g_selectionReady = false;
var g_entityTypes = [];
var g_entityTypesPromise;
var g_selectionCache = {};

function selectionStatus(message) {
    $('#manageUsersSelectionStatus').text(message);
}

function entityTypeLabel(id) {
    var row = g_entityTypes.filter(function (item) { return toInt(item.autid) === toInt(id); })[0];
    return row ? row.entitytypedesc : '';
}

function resetSelection(selector, message) {
    $(selector).empty().append($('<option>').val(0).text(message)).prop('disabled', true);
}

function selectionRequest(action, data) {
    return $.ajax({
        url: g_asiBaseURL + '/ApiCalls/' + action,
        type: 'POST', data: data || {}, dataType: 'json', cache: false,
        headers: { RequestVerificationToken: $('#manageUsersSelectionCsrf input[name="__RequestVerificationToken"]').val() }
    });
}

function selectionMapping(action, data) {
    var key = action + ':' + JSON.stringify(data);
    if (!g_selectionCache[key]) {
        g_selectionCache[key] = selectionRequest(action, data).catch(function (error) {
            delete g_selectionCache[key];
            throw error;
        });
    }
    return g_selectionCache[key];
}

function fillSelection(selector, rows, idField, textField, placeholder, selected) {
    resetSelection(selector, rows.length ? placeholder : 'No available selections');
    $.each(rows, function (_, row) {
        $(selector).append($('<option>').val(row[idField]).text(row[textField]));
    });
    $(selector).prop('disabled', !rows.length).val(selected || 0);
}

function initializeEntitySelection() {
    resetSelection('#controlingsearch', '--Select Controlling/Reporting Office--');
    resetSelection('#childposting', '--Select Place of Posting--');
    $(document).off('change.manageUsersSelection')
        .on('change.manageUsersSelection', '#EntityTypeField', function () { loadReportingSelection(); })
        .on('change.manageUsersSelection', '#controlingsearch', function () { loadPostingSelection(); })
        .on('change.manageUsersSelection', '#childposting', function () {
            g_selectionReady = toInt($(this).val()) > 0;
            selectionStatus('');
        });
    var element = document.getElementById('EntityTypeField');
    g_entityTypesPromise = selectionRequest('GetEntityTypesForSelection').then(function (rows) {
        if (document.getElementById('EntityTypeField') !== element) return [];
        g_entityTypes = rows;
        fillSelection('#EntityTypeField', rows, 'autid', 'entitytypedesc', '--Select Entity Type--');
        renderUserContexts();
        selectionStatus(rows.length ? '' : 'No entity types are available for your session.');
        return rows;
    }, function () {
        if (document.getElementById('EntityTypeField') === element) {
            resetSelection('#EntityTypeField', 'Unable to load entity types');
            selectionStatus('Entity type lookup failed. Reload Manage Users to retry.');
        }
        return [];
    });
}

async function loadReportingSelection(parentId, postingId, version) {
    version = version === undefined ? ++g_selectionVersion : version;
    g_selectionReady = false;
    resetSelection('#controlingsearch', 'Loading reporting offices...');
    resetSelection('#childposting', '--Select Place of Posting--');
    var typeId = toInt($('#EntityTypeField').val());
    if (!typeId) {
        resetSelection('#controlingsearch', '--Select Controlling/Reporting Office--');
        selectionStatus('');
        return;
    }
    try {
        var rows = await selectionMapping('GetReportingOfficesForSelection', { entityTypeId: typeId });
        if (version !== g_selectionVersion) return;
        fillSelection('#controlingsearch', rows, 'entitY_ID', 'description', '--Select Controlling/Reporting Office--', parentId);
        selectionStatus(rows.length ? '' : 'No reporting offices are available for this entity type.');
        if (parentId) {
            if (!rows.some(function (row) { return toInt(row.entitY_ID) === parentId; })) throw new Error('unresolved');
            await loadPostingSelection(postingId, version);
        }
    } catch (_) {
        if (version !== g_selectionVersion) return;
        selectionStatus(parentId ? 'Existing assignment could not be resolved. It remains unchanged in the grid. Retry the lookup before updating it.' : 'Unable to load reporting offices. Select the entity type again to retry.');
    }
}

async function loadPostingSelection(postingId, version) {
    version = version === undefined ? ++g_selectionVersion : version;
    g_selectionReady = false;
    resetSelection('#childposting', 'Loading postings...');
    var parentId = toInt($('#controlingsearch').val());
    if (!parentId) {
        resetSelection('#childposting', '--Select Place of Posting--');
        selectionStatus('');
        return;
    }
    try {
        var rows = await selectionMapping('GetPostingsForSelection', { reportingId: parentId, entityTypeId: toInt($('#EntityTypeField').val()) });
        if (version !== g_selectionVersion) return;
        fillSelection('#childposting', rows, 'entitY_ID', 'c_NAME', '--Select Place of Posting--', postingId);
        if (postingId && !rows.some(function (row) { return toInt(row.entitY_ID) === postingId; })) throw new Error('unresolved');
        g_selectionReady = toInt($('#childposting').val()) > 0;
        selectionStatus(rows.length ? '' : 'No postings are available for this reporting office.');
    } catch (_) {
        if (version !== g_selectionVersion) return;
        selectionStatus(postingId ? 'Existing posting could not be resolved. The assignment remains unchanged in the grid.' : 'Unable to load postings. Select the reporting office again to retry.');
    }
}

// Resolve missing context types only from the authoritative posting cursor TYPEID.
async function resolveContextEntityType(context) {
    if (context.entityTypeId > 0 && context.parentEntityId > 0) return context.entityTypeId;
    await g_entityTypesPromise;
    for (var i = 0; i < g_entityTypes.length; i++) {
        var typeId = toInt(g_entityTypes[i].autid);
        if (context.entityTypeId > 0 && context.entityTypeId !== typeId) continue;
        var offices = await selectionMapping('GetReportingOfficesForSelection', { entityTypeId: typeId });
        var candidates = offices.filter(function (office) {
            return !context.parentEntityId || toInt(office.entitY_ID) === context.parentEntityId;
        });
        for (var j = 0; j < candidates.length; j++) {
            var parentId = toInt(candidates[j].entitY_ID);
            var postings = await selectionMapping('GetPostingsForSelection', { reportingId: parentId, entityTypeId: typeId });
            var posting = postings.filter(function (row) { return toInt(row.entitY_ID) === context.entityId; })[0];
            if (posting) {
                context.entityTypeId = toInt(posting.typeid);
                context.entityTypeName = entityTypeLabel(context.entityTypeId);
                context.selectionParentId = parentId;
                return context.entityTypeId;
            }
        }
    }
    return 0;
}

async function restoreEntitySelection(context) {
    var version = ++g_selectionVersion;
    g_selectionReady = false;
    $('#EntityTypeField').val(0);
    resetSelection('#controlingsearch', 'Resolving assignment...');
    resetSelection('#childposting', '--Select Place of Posting--');
    selectionStatus('Resolving existing assignment...');
    try {
        await g_entityTypesPromise;
        var typeId = await resolveContextEntityType(context);
        if (version !== g_selectionVersion) return;
        renderUserContexts();
        if (!typeId || !entityTypeLabel(typeId)) throw new Error('unresolved');
        $('#EntityTypeField').val(typeId);
        await loadReportingSelection(context.selectionParentId || context.parentEntityId, context.entityId, version);
    } catch (_) {
        if (version !== g_selectionVersion) return;
        selectionStatus('Existing assignment could not be resolved. It remains unchanged in the grid.');
    }
}
