(function () {
  function buildRegex({ allowAmp = true, allowQuestion = true } = {}) {
    const allowedChars = `A-Za-z0-9, ${allowAmp ? "&" : ""}${allowQuestion ? "\\?" : ""}`;
    return new RegExp(`^[${allowedChars}]*$`);
  }

  function sanitizeAlnum(value, { allowAmp = true, allowQuestion = true } = {}) {
    if (value == null) return "";
    let sanitized = String(value);
    const allowedChars = `A-Za-z0-9, ${allowAmp ? "&" : ""}${allowQuestion ? "\\?" : ""}`;
    const pattern = new RegExp(`[^${allowedChars}]+`, "g");
    sanitized = sanitized.replace(pattern, "");
    return sanitized;
  }

  function attachAlnumOnly(selector, { allowAmp = true, allowQuestion = true, maxLen = null } = {}) {
    document.querySelectorAll(selector).forEach(el => {
      const elementAllowAmp = el.dataset.allowAmp === "false" ? false : allowAmp;
      const elementAllowQuestion = el.dataset.allowQuestion === "true" ? true : allowQuestion;
      const elementMaxLen = el.dataset.maxlen ? parseInt(el.dataset.maxlen, 10) : maxLen;
      const regex = buildRegex({ allowAmp: elementAllowAmp, allowQuestion: elementAllowQuestion });

      el.setAttribute("autocomplete", "off");

      el.addEventListener("input", () => {
        const clean = sanitizeAlnum(el.value, { allowAmp: elementAllowAmp, allowQuestion: elementAllowQuestion });
        if (elementMaxLen && !Number.isNaN(elementMaxLen) && clean.length > elementMaxLen) {
          el.value = clean.substring(0, elementMaxLen);
        } else {
          el.value = clean;
        }
        if (el.value === "") {
          el.classList.remove("is-invalid");
          return;
        }
        el.classList.toggle("is-invalid", !regex.test(el.value));
      });

      el.addEventListener("blur", () => {
        el.value = sanitizeAlnum(el.value, { allowAmp: elementAllowAmp, allowQuestion: elementAllowQuestion });
        if (el.value === "") {
          el.classList.remove("is-invalid");
          return;
        }
        el.classList.toggle("is-invalid", !regex.test(el.value));
      });
    });
  }

  function isAlnumOk(selector, { allowAmp = true, allowQuestion = true, required = false } = {}) {
    const el = document.querySelector(selector);
    if (!el) return true;

    const regex = buildRegex({ allowAmp, allowQuestion });
    const rawVal = el.value || "";
    const value = sanitizeAlnum(rawVal, { allowAmp, allowQuestion });
    const isEmpty = value.trim() === "";

    if (required && isEmpty) {
      el.classList.add("is-invalid");
      return false;
    }

    const isValid = isEmpty ? !required : regex.test(value);
    el.classList.toggle("is-invalid", !isValid);
    if (isValid && value !== rawVal) {
      el.value = value;
    }
    return isValid;
  }

  const forbiddenCharsPattern = /[<>@=:\\"]/g;
  const plainTextForbiddenPattern = /[<>'"&]/g;
  const htmlTagPattern = /<[^>]*?>/g;

  function sanitizeNoAngleBrackets(value) {
    if (value == null) return "";
    return String(value).replace(forbiddenCharsPattern, "");
  }

  function sanitizeNoAngleBracketsAllowAt(value, allowAt) {
    if (!allowAt) return sanitizeNoAngleBrackets(value);
    if (value == null) return "";
    const placeholder = "__ALLOW_AT__";
    const rawValue = String(value).replace(/@/g, placeholder);
    const sanitized = rawValue.replace(forbiddenCharsPattern, "");
    return sanitized.replace(new RegExp(placeholder, "g"), "@");
  }

  function sanitizePlainText(value) {
    if (value == null) return "";
    let sanitized = String(value);
    sanitized = sanitized.replace(htmlTagPattern, "");
    sanitized = sanitized.replace(plainTextForbiddenPattern, "");
    return sanitized;
  }

  function toggleAngleBracketInvalidState(el, removed, allowAt) {
    const defaultMessage = allowAt
      ? "The characters <, >, =, :, \\, and \" are not allowed."
      : "The characters <, >, =, @, :, \\, and \" are not allowed.";
    const message = el.dataset.noAngleMessage || defaultMessage;
    if (removed) {
      el.classList.add("is-invalid");
      if (!el.getAttribute("title")) {
        el.setAttribute("title", message);
      }
    } else {
      el.classList.remove("is-invalid");
      if (el.getAttribute("title") === message) {
        el.removeAttribute("title");
      }
    }
  }

  function togglePlainTextInvalidState(el, removed) {
    const message =
      el.dataset.plainTextMessage ||
      `Only plain text is allowed. Characters such as <, >, ', ", &, and HTML tags are removed.`;

    if (removed) {
      el.classList.add("is-invalid");
      if (!el.getAttribute("title")) {
        el.setAttribute("title", message);
      }
    } else {
      el.classList.remove("is-invalid");
      if (el.getAttribute("title") === message) {
        el.removeAttribute("title");
      }
    }
  }

  function sanitizeElementNoAngleBrackets(el) {
    const isContentEditable = el.getAttribute("contenteditable") === "true";
    const currentValue = isContentEditable ? el.textContent || "" : el.value || "";
    const allowAt = el.dataset.allowAt === "true";
    const sanitized = sanitizeNoAngleBracketsAllowAt(currentValue, allowAt);
    const removed = currentValue !== sanitized;

    if (removed) {
      if (isContentEditable) {
        el.textContent = sanitized;
      } else {
        el.value = sanitized;
      }
    }

    toggleAngleBracketInvalidState(el, removed, allowAt);
  }

  function sanitizeElementPlainText(el) {
    const isContentEditable = el.getAttribute("contenteditable") === "true";
    const currentValue = isContentEditable ? el.textContent || "" : el.value || "";
    const sanitized = sanitizePlainText(currentValue);
    const removed = currentValue !== sanitized;

    if (removed) {
      if (isContentEditable) {
        el.textContent = sanitized;
      } else {
        el.value = sanitized;
      }
    }

    togglePlainTextInvalidState(el, removed);
  }

  function attachNoAngleBrackets(selector) {
    document.querySelectorAll(selector).forEach(el => {
      const handler = () => sanitizeElementNoAngleBrackets(el);
      el.addEventListener("input", handler);
      el.addEventListener("paste", () => {
        setTimeout(handler, 0);
      });
    });
  }

  function attachPlainTextValidation(selector = '[data-validate="plain-text"]') {
    document.querySelectorAll(selector).forEach(el => {
      const handler = () => sanitizeElementPlainText(el);
      ["input", "blur"].forEach(evt => el.addEventListener(evt, handler));
      el.addEventListener("paste", () => {
        setTimeout(handler, 0);
      });
      handler();
    });
  }

  function isAuditPeriodPage() {
    const path = (window.location && window.location.pathname) || "";
    return path.toLowerCase().includes("/planning/audit_period");
  }

  function isDateLikeInput(el) {
    if (!el || el.tagName !== "INPUT") return false;
    if ((el.type || "").toLowerCase() === "hidden") return false;
    if ((el.type || "").toLowerCase() === "date") return true;

    const datePattern = /(\bdate\b|\bdt\b|fromdate|todate|startdate|enddate|from_date|to_date|from_dt|to_dt)/i;
    const identifier = `${el.id || ""} ${el.name || ""} ${el.className || ""}`;
    return datePattern.test(identifier);
  }

  function normalizeDateFromString(value) {
    if (!value) return null;
    const trimmed = value.trim();
    if (!trimmed) return null;

    const monthYearMatch = trimmed.match(/^(\d{1,2})[-/](\d{4})$/);
    if (monthYearMatch) {
      const month = parseInt(monthYearMatch[1], 10) - 1;
      const year = parseInt(monthYearMatch[2], 10);
      const date = new Date(year, month, 1);
      return Number.isNaN(date.getTime()) ? null : date;
    }

    const slashParts = trimmed.split(/[\/\-\.]/);
    if (slashParts.length === 3) {
      const [p1, p2, p3] = slashParts;
      let day = null;
      let month = null;
      let year = null;

      if (p1.length === 4) {
        year = parseInt(p1, 10);
        month = parseInt(p2, 10) - 1;
        day = parseInt(p3, 10);
      } else {
        day = parseInt(p1, 10);
        month = parseInt(p2, 10) - 1;
        year = parseInt(p3, 10);
      }

      const parsed = new Date(year, month, day);
      if (!Number.isNaN(parsed.getTime())) return parsed;
    }

    const parsedTs = Date.parse(trimmed);
    if (!Number.isNaN(parsedTs)) {
      return new Date(parsedTs);
    }

    return null;
  }

  function flagFutureDate(el, todayDate) {
    if (!el) return true;
    if (el.dataset.allowFuture === "true") return true;

    const rawValue = (el.value || "").trim();
    if (!rawValue) {
      el.classList.remove("is-invalid");
      if (typeof el.setCustomValidity === "function") {
        el.setCustomValidity("");
      }
      return true;
    }

    const parsedDate = normalizeDateFromString(rawValue);
    if (!parsedDate) {
      el.classList.remove("is-invalid");
      if (typeof el.setCustomValidity === "function") {
        el.setCustomValidity("");
      }
      return true;
    }

    const isFuture = parsedDate > todayDate;
    const message = el.dataset.futureMessage || "Future dates are not allowed.";

    if (typeof el.setCustomValidity === "function") {
      el.setCustomValidity(isFuture ? message : "");
    }

    el.classList.toggle("is-invalid", isFuture);

    if (isFuture && typeof el.reportValidity === "function") {
      el.reportValidity();
    }

    return !isFuture;
  }

  function hintNoFuture(el) {
    if (!el || el.dataset.allowFuture === "true") return;
    if (el.placeholder && !/(today)/i.test(el.placeholder)) {
      el.placeholder = `${el.placeholder} (Up to today only)`;
    }
  }

  function limitDatepicker(el, todayDate, todayStr) {
    if (!window.jQuery || typeof window.jQuery.fn.datepicker !== "function") return;
    if (el.dataset.allowFuture === "true") return;

    const $el = window.jQuery(el);

    try {
      if (typeof $el.datepicker === "function") {
        try {
          $el.datepicker("setEndDate", todayStr);
        } catch (err) {
          // setEndDate is available in some datepicker plugins (e.g., bootstrap-datepicker).
        }

        try {
          $el.datepicker("option", "maxDate", todayDate);
        } catch (e) {
          // If the plugin does not support option, ignore.
        }
      }
    } catch (e) {
      // Silently ignore datepicker configuration errors to avoid breaking the page.
    }
  }

  function attachNoFutureDates() {
    if (isAuditPeriodPage()) return;

    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const todayStr = today.toISOString().split("T")[0];

    const candidates = Array.from(document.querySelectorAll("input")).filter(isDateLikeInput);
    candidates.forEach(el => {
      if (el.dataset.allowFuture === "true") return;

      if ((el.type || "").toLowerCase() === "date") {
        el.setAttribute("max", todayStr);
      }

      hintNoFuture(el);

      const handler = () => flagFutureDate(el, today);
      ["input", "change", "blur"].forEach(evt => el.addEventListener(evt, handler));

      handler();

      limitDatepicker(el, today, todayStr);

      if (window.jQuery && typeof window.jQuery.fn.datepicker === "function") {
        const $el = window.jQuery(el);
        $el.on("focus", () => limitDatepicker(el, today, todayStr));
        $el.on("show", () => limitDatepicker(el, today, todayStr));
      }
    });
  }

  window.CommonValidation = {
    attachAlnumOnly,
    sanitizeAlnum,
    isAlnumOk,
    sanitizeNoAngleBrackets,
    attachNoAngleBrackets,
    attachNoFutureDates,
    attachPlainTextValidation,
    sanitizePlainText
  };
})();

document.addEventListener("DOMContentLoaded", function () {
  if (!window.CommonValidation) return;

  const plainTextCandidates = document.querySelectorAll(
    'input[type="text"]:not([data-validate]):not([data-allow-html="true"]), input[type="search"]:not([data-validate]):not([data-allow-html="true"]), textarea:not([data-validate]):not([data-allow-html="true"])'
  );

  plainTextCandidates.forEach(el => {
    const className = (el.className || "").toLowerCase();
    const isRichText =
      className.includes("richtext") ||
      className.includes("rich-text") ||
      className.includes("content") ||
      el.dataset.richText === "true";

    if (!isRichText) {
      el.dataset.validate = "plain-text";
    }
  });

  CommonValidation.attachNoAngleBrackets("input:not([type=password]):not([type=hidden]), textarea, [contenteditable=\"true\"]");

  if (CommonValidation.attachAlnumOnly) {
    CommonValidation.attachAlnumOnly("input.alnum-only");
  }

  if (CommonValidation.attachNoFutureDates) {
    CommonValidation.attachNoFutureDates();
  }

  if (CommonValidation.attachPlainTextValidation) {
    CommonValidation.attachPlainTextValidation();
  }
});
