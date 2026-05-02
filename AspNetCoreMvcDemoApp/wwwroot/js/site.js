class AppError extends Error {
    constructor(message, code = 500) {
        super(message);
        this.name = this.constructor.name;
        this.code = code;
        this.isOperational = true;
        this.isHandled = false;
        Error.captureStackTrace(this, this.constructor);
    }
}

class UrlRequiredError extends AppError {
    constructor(methodName = "The operation") {
        super(`${methodName} requires a valid URL.`, 400);
    }
}

class ApiError extends AppError {
    constructor(result, response) {
        const shortMsg = `Server Error: ${response.statusText} (${response.status})`;
        super(shortMsg, response.status);
        this.result = result;
        this.response = response;
        this.isHandled = true;
    }
}

class NetworkError extends AppError {
    constructor(originalError) {
        const userMsg = "Unable to connect to the server. Please check your internet connection and try again.";
        super(userMsg, 0);

        this.name = "NetworkError";
        this.originalError = originalError;
        this.isHandled = true;
    }
}

(() => {
    const _config = {
        locale: 'tr-TR',
        currency: 'TRY',

        dateOptions: {
            year: 'numeric', month: '2-digit', day: '2-digit'
        },

        dateTimeOptions: {
            year: 'numeric', month: '2-digit', day: '2-digit',
            hour: '2-digit', minute: '2-digit', second: '2-digit'
        },

        get numberFormatter() {
            return new Intl.NumberFormat(this.locale, {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
        },

        get currencyFormatter() {
            return new Intl.NumberFormat(this.locale, {
                style: 'currency',
                currency: this.currency
            });
        }
    };

    const format = (value, dataFormat) => {
        if (value === null || value === undefined) return "-";
        if (!dataFormat) return value;

        if (dataFormat === 'date' || dataFormat === 'datetime') {
            value = (value instanceof Date) ? value : new Date(value);
        }

        switch (dataFormat) {
            case 'date':
                return value.toLocaleDateString(_config.locale, _config.dateOptions);
            case 'datetime':
                return value.toLocaleString(_config.locale, _config.dateTimeOptions);
            case 'currency':
                return _config.currencyFormatter.format(value);
            default:
                return value;
        }
    };

    const _busyState = Vue.reactive({ active: false });
    const busy = {
        show: () => { _busyState.active = true; },
        hide: () => { _busyState.active = false; },
        get isActive() {
            return _busyState.active;
        }
    };

    const _modalState = Vue.reactive({
        title: '',
        message: '',
        isSuccess: false,
        isConfirm: false,
        isVisible: false,
        isArray: false,

        // Internal resolver for confirm promises
        _resolve: null
    });

    const modal = {
        state: _modalState,

        success(msg) {
            this._show({ message: msg, title: 'Successful', isSuccess: true });
        },

        fail(msg) {
            this._show({ message: msg, title: 'Error', isSuccess: false });
        },

        confirm(optionsOrMessage) {
            this._reset();

            const opts = typeof optionsOrMessage === 'string'
                ? { message: optionsOrMessage }
                : optionsOrMessage;

            this._show({
                message: opts.message || "Do you approve the operation?",
                title: opts.title || "Approval Required",
                isConfirm: true
            });

            return new Promise((res) => {
                this.state._resolve = res;
            });
        },

        _show({ message, title, isSuccess = false, isConfirm = false }) {
            this.state.message = message;
            this.state.title = title;
            this.state.isSuccess = isSuccess;
            this.state.isConfirm = isConfirm;
            this.state.isArray = Array.isArray(message);
            this.state.isVisible = true;
        },

        answer(val) {
            this.state.isVisible = false;

            const resolve = this.state._resolve;
            this._reset();

            if (typeof resolve === 'function') {
                resolve(val);
            }
        },

        _reset() {
            this.state._resolve = null;
            this.state.isConfirm = false;
            this.state.isSuccess = false;
            this.state.isArray = false;
        },

        hide() {
            this.answer(false);
        }
    };


    const http = {
        _handleError(result, response) {
            let errorArray = [];
            if (result && typeof result === 'object') {
                if (result.title) errorArray.push(`<strong>Title:</strong> ${result.title}`);
                if (result.detail) errorArray.push(`<strong>Detail:</strong> ${result.detail}`);

                if (result.validationErrors && Array.isArray(result.validationErrors)) {
                    let listHtml = `<ol class="list-group list-group-numbered mt-2">`;
                    result.validationErrors.forEach(msg => {
                        listHtml += `<li class="list-group-item list-group-item-danger">${msg}</li>`;
                    });
                    listHtml += `</ol>`;
                    errorArray.push(listHtml);
                }
                else if (response.status !== 400) {
                    ['status', 'type', 'instance', 'traceId'].forEach(key => {
                        if (result[key]) {
                            errorArray.push(`<small class="text-muted" style="display:block;"><strong>${key.toUpperCase()}:</strong> ${result[key]}</small>`);
                        }
                    });
                }
            }

            const err = new ApiError(result, response);
            if (errorArray.length > 0) {
                err.isHandled = true;
                modal.fail(errorArray);
            }

            return err;
        },

        async get(options) {
            return await this.send({ ...options, method: 'GET' });
        },

        async post(options) {
            return await this.send({ ...options, method: 'POST' });
        },

        async send({ url, data, method = 'POST' }) {

            if (busy.isActive) {
                modal.fail("A request is already in progress. Please wait.");
                return;
            }

            try {
                busy.show();

                const headers = {
                    'Accept': 'application/json, application/problem+json, text/plain, */*',
                    'Cache-Control': 'no-cache, no-store, must-revalidate',
                    'Pragma': 'no-cache',
                    'Expires': '0'
                };

                let body = null;
                let finalUrl = url;

                if (method === 'GET' && data) {
                    const queryString = new URLSearchParams(data).toString();
                    finalUrl += (finalUrl.includes('?') ? '&' : '?') + queryString;
                }
                else if (method === 'POST' && data) {
                    if (data instanceof FormData) {
                        body = data;
                    }
                    else if (Array.isArray(data) || (typeof data === 'object' && data !== null)) {
                        headers['Content-Type'] = 'application/json; charset=UTF-8';
                        body = JSON.stringify(data);
                    }
                    else {
                        headers['Content-Type'] = 'application/x-www-form-urlencoded; charset=UTF-8';
                        body = new URLSearchParams(data).toString();
                    }
                }

                const response = await fetch(finalUrl, {
                    method: method,
                    headers: headers,
                    body: body,
                    cache: 'no-store'
                });

                const contentType = response.headers.get("content-type");
                const result = contentType && (contentType.includes("application/json") || contentType.includes("application/problem+json"))
                    ? await response.json()
                    : await response.text();

                if (!response.ok) {
                    throw this._handleError(result, response);
                }

                if (result) {
                    if (result.redirectUrl) {
                        window.location.href = result.redirectUrl;
                        return result;
                    }
                    if (result.detail) {
                        modal.success(result.detail);
                    }
                }

                return result;
            } catch (error) {
                console.error("HTTP Layer Catch:", error);

                if (error.isHandled) {
                    throw error;
                }

                const networkError = new NetworkError(error);
                modal.fail([
                    `<strong>Connection Error</strong>`,
                    `<div>${networkError.message}</div>`,
                    `<small class="text-muted">Details: ${error.message || "Unknown network failure"}</small>`
                ]);

                throw networkError;
            }
            finally {
                busy.hide();
            }
        }
    };

    const tableManager = (options = {}) => {
        const { ref, reactive, computed, nextTick } = Vue;

        const config = {
            data: { items: [], totalCount: 0, pageNumber: 1, pageSize: 0, totalPages: 0, hasRecords: false },
            columns: [],
            searchModel: {},
            loadUrl: '',
            primaryKey: 'id',
            isSelectable: (row) => true,
            ...options
        };

        config.columns.forEach(col => {
            col.is = (name) => {
                if (!name) return false;

                if (Array.isArray(name)) {
                    return name.some(n =>
                        n === col.propName ||
                        n === col.name
                    );
                }

                return col.propName === name || col.name === name;
            };
        });

        const _initialFilters = JSON.parse(JSON.stringify(config.searchModel));
        const data = reactive(config.data);
        const filters = reactive(config.searchModel);

        const _prepareData = (payload) => {
            return JSON.parse(JSON.stringify(payload, (key, value) => {
                if (typeof value === 'string') {
                    const trimmedValue = value.trim();
                    return trimmedValue === "" ? null : trimmedValue;
                }
                return value;
            }));
        };

        const selection = reactive({
            items: [],
            isAllSelected: computed(() => {
                const selectable = data.items.filter(config.isSelectable);
                if (selectable.length === 0) return false;
                return selection.items.length === selectable.length;
            }),
            toggleAll: (event) => {
                selection.items = event.target.checked
                    ? data.items.filter(config.isSelectable).map(i => i[config.primaryKey])
                    : [];
            },
            count: computed(() => selection.items.length),
            isSelected: (id) => selection.items.includes(id)
        });

        const fetchData = async (isReset = false, customAjaxOptions = null) => {
            if (busy.isActive) return;

            try {
                if (isReset) filters.page = 1;

                const requestConfig = customAjaxOptions
                    ?{
                        ...customAjaxOptions,
                        data: _prepareData(customAjaxOptions.data)
                    }
                    :{
                        url: config.loadUrl,
                        data: _prepareData(filters)
                    };

                const result = await http.post(requestConfig);
                if (result) {

                    const { items, ...metaData } = result;

                    if (isReset) {
                        Object.assign(data, result);
                    } else {
                        Object.assign(data, metaData);
                        data.items.push(...(items || []));
                    }

                    if (customAjaxOptions && typeof customAjaxOptions.onSuccess === 'function') {
                        customAjaxOptions.onSuccess(result);
                    }
                }

            } finally {
                if (selection.items.length > 0) selection.items = [];
            }
        };

        const actions = reactive({
            delete: async (options = {}) => {

                const settings = {
                    event: null,
                    callback: null,
                    url: null,
                    requreText: null,
                    confirmText: null,
                    ...options
                };

                const btn = settings.event?.currentTarget;
                const requestUrl = settings.url || btn?.dataset.url || btn?.getAttribute('href');
                if (!requestUrl) {
                    throw new UrlRequiredError("delete");
                }

                if (selection.count === 0) {
                    modal.fail(settings.requreText || "Please select records to delete.");
                    return;
                }

                const isConfirmed = await modal.confirm(settings.confirmText || `Are you sure you want to delete ${selection.count} records?`);
                if (isConfirmed) {

                    const fetchOptions = {
                        url: requestUrl,
                        data: {
                            ids: selection.items,
                            searchDto: filters
                        },
                        onSuccess: settings.callback
                    };

                    await fetchData(true, fetchOptions);
                }
            },
            filterReset: async () => {
                Object.keys(filters).forEach(key => {
                    filters[key] = _initialFilters[key];
                });
                await fetchData(true);
            },
            search: async (options = {}) => {

                const settings = {
                    event: null,
                    callback: null,
                    url: null,
                    ...options
                };

                const form = settings.event?.currentTarget;
                const requestUrl = settings.url || form?.dataset.url || form?.getAttribute('action');
                if (!requestUrl) {
                    throw new UrlRequiredError("search");
                }

                const fetchOptions = {
                    url: requestUrl,
                    data: filters,
                    onSuccess: settings.callback
                };

                await fetchData(true, fetchOptions);
            }
        });

        const sorting = reactive({
            getClass: (colName) => {
                if (filters.sortColumn !== colName) return 'table-header-sort';
                return `table-header-sort sort-${filters.sortDirection}`;
            },
            apply: async (colName) => {
                if (filters.sortColumn === colName) {
                    filters.sortDirection = filters.sortDirection === 'asc' ? 'desc' : 'asc';
                } else {
                    filters.sortColumn = colName;
                    filters.sortDirection = 'asc';
                }
                await fetchData(true);
            }
        });

      
        const pagination = reactive({
            hasMore: computed(() => data.items.length > 0 && data.pageNumber < data.totalPages),
            text: computed(() => {
                return pagination.hasMore ? `Load More (${data.pageNumber} / ${data.totalPages})` : 'All Records Loaded';
            }),
            load: async (event) => { 
                if (data.pageNumber < data.totalPages && !busy.isActive) {
                    const clickedButton = event.currentTarget;
                    filters.page = data.pageNumber + 1;

                    await fetchData(false);
                    await nextTick();

                    if (clickedButton) {
                        clickedButton.scrollIntoView({
                            behavior: 'smooth',
                            block: 'end'
                        });
                    }
                }
            }
        });

        return reactive({
            columns: config.columns,
            data: data,
            filters,
            actions,
            selection,
            isSelectable: config.isSelectable,
            sorting,
            pagination
        });
    };


    const createBaseApp = () => {
        const rootComponent = {
            setup() {
                const setupFn = typeof window.pageInit === 'function' ? window.pageInit : () => ({});
                const pageContext = setupFn() || {};
                return {
                    ...pageContext,
                    modal: window.core.modal,
                    config: window.core._config,
                    utils: window.core.utils
                };
            }
        };

        const app = Vue.createApp(rootComponent);

        app.directive('confirm', {
            mounted(el, binding) {
                el._vueConfirm = binding.value;
            }
        });

        app.directive('autopost', {
            mounted(form, binding) {

                form.addEventListener('submit', async (e) => {
                    e.preventDefault();

                    try {
                        const btn = e.submitter;
                        const formData = new FormData(form);

                        const settings = {
                            formType: 'default',
                            args: null,
                            callback: null,
                            url: null,
                            ...binding.value
                        };

                        const url = settings.url || form.getAttribute("action");
                        const isValid = url && (
                            url.startsWith("/") ||
                            url.startsWith("~") ||
                            url.startsWith("http")
                        );

                        if (!isValid) {
                            const errorMsg = `[v-autopost] Invalid URL detected! Target: ${form.tagName}. Found: "${url}"`;
                            console.error(errorMsg, el);
                            throw new Error(errorMsg);
                        }

                        if (settings.formType === "multiple-upload" && typeof settings.args === 'function') {
                            const filesRef = settings.args();
                            const selectedFiles = Vue.isRef(filesRef) ? filesRef.value : filesRef;

                            if (Array.isArray(selectedFiles) && selectedFiles.length > 0) {
                                formData.delete("Items");
                                selectedFiles.forEach((item, i) => {
                                    formData.append(`Items[${i}].File`, item.raw);
                                    formData.append(`Items[${i}].Description`, item.description);
                                });
                            }
                        }

                        const result = await http.post({
                            url: url,
                            data: formData
                        });

                        if (result && typeof settings.callback === 'function') {
                            settings.callback(result);
                        }

                    } catch (error) {
                        console.error("Autopost Error:", error);
                    }
                });
            }
        });

        return app;
    };

    // GLOBAL EXPORT
    window.core = {
        utils: { format, busy, http },
        modal: modal,
        table: tableManager,
        createApp: createBaseApp
    };
})();