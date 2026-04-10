/** Scrolls an element to its bottom — called from Blazor via JS interop */
function scrollToBottom(element) {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
}

/** Sets focus on an element — called from Blazor via JS interop */
function focusElement(element) {
    if (element) {
        element.focus();
    }
}
