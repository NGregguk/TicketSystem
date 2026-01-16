(() => {
  const insertText = (textarea, text) => {
    const start = textarea.selectionStart ?? textarea.value.length;
    const end = textarea.selectionEnd ?? textarea.value.length;
    const before = textarea.value.substring(0, start);
    const after = textarea.value.substring(end);
    textarea.value = `${before}${text}${after}`;
    const cursor = start + text.length;
    textarea.selectionStart = cursor;
    textarea.selectionEnd = cursor;
    textarea.focus();
  };

  const createPreviewItem = (url, fileName) => {
    const item = document.createElement("div");
    item.className = "paste-preview-item";
    const img = document.createElement("img");
    img.src = url;
    img.alt = fileName || "Pasted image";
    const name = document.createElement("div");
    name.className = "paste-preview-name";
    name.textContent = fileName || "Pasted image";
    item.appendChild(img);
    item.appendChild(name);
    return item;
  };

  const showInlineMessage = (container, message, variant) => {
    if (!container) {
      return;
    }
    const banner = document.createElement("div");
    banner.className = `paste-message ${variant || "info"}`;
    banner.textContent = message;
    container.appendChild(banner);
    window.setTimeout(() => {
      banner.remove();
    }, 4000);
  };

  window.initPasteUpload = (options) => {
    if (!options) {
      return;
    }

    const textarea = document.querySelector(options.textareaSelector);
    if (!textarea) {
      return;
    }

    const preview = options.previewSelector
      ? document.querySelector(options.previewSelector)
      : null;
    const tempKeyInput = options.tempKeySelector
      ? document.querySelector(options.tempKeySelector)
      : null;
    const uploadUrl = options.uploadUrl;
    const antiForgeryToken = document.querySelector("input[name='__RequestVerificationToken']")?.value;

    if (!uploadUrl) {
      return;
    }

    textarea.addEventListener("paste", async (event) => {
      const clipboard = event.clipboardData;
      if (!clipboard || !clipboard.items) {
        return;
      }

      const images = [];
      for (const item of clipboard.items) {
        if (item.type && item.type.startsWith("image/")) {
          const file = item.getAsFile();
          if (file) {
            images.push(file);
          }
        }
      }

      if (!images.length) {
        return;
      }

      event.preventDefault();
      const text = clipboard.getData("text");
      if (text) {
        insertText(textarea, text);
      }

      for (const file of images) {
        const formData = new FormData();
        formData.append("file", file, file.name || "pasted-image.png");
        if (tempKeyInput) {
          formData.append("tempKey", tempKeyInput.value);
        }

        try {
          const response = await fetch(uploadUrl, {
            method: "POST",
            body: formData,
            headers: {
              "X-Requested-With": "XMLHttpRequest",
              "RequestVerificationToken": antiForgeryToken || ""
            }
          });

          if (!response.ok) {
            const errorPayload = await response.json().catch(() => null);
            const message = errorPayload?.error || "Upload failed.";
            showInlineMessage(preview, message, "error");
            continue;
          }

          const payload = await response.json();
          if (payload?.attachmentId) {
            insertText(textarea, `\n[[image:${payload.attachmentId}]]\n`);
            if (preview) {
              preview.appendChild(createPreviewItem(payload.url, payload.fileName));
            }
          }
        } catch (error) {
          showInlineMessage(preview, "Upload failed.", "error");
        }
      }
    });
  };
})();
