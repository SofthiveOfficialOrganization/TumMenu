(function () {
  function renumberRows(editor) {
    editor.querySelectorAll(".js-social-link-row").forEach(function (row, index) {
      row.querySelectorAll("[data-social-field]").forEach(function (field) {
        field.name = "SocialLinks[" + index + "]." + field.dataset.socialField;
      });
    });
  }

  function initEditor(editor) {
    var list = editor.querySelector(".js-social-link-list");
    var template = editor.querySelector(".js-social-link-template");
    var addButton = editor.querySelector(".js-add-social-link");

    if (!list || !template || !addButton) {
      return;
    }

    addButton.addEventListener("click", function () {
      var node = template.content.firstElementChild.cloneNode(true);
      list.appendChild(node);
      renumberRows(editor);
      var urlInput = node.querySelector('[data-social-field="Url"]');
      if (urlInput) {
        urlInput.focus();
      }
    });

    editor.addEventListener("click", function (event) {
      var removeButton = event.target.closest(".js-remove-social-link");
      if (!removeButton) {
        return;
      }

      var row = removeButton.closest(".js-social-link-row");
      if (row) {
        row.remove();
        renumberRows(editor);
      }
    });

    renumberRows(editor);
  }

  document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".js-social-links-editor").forEach(initEditor);
  });
})();
