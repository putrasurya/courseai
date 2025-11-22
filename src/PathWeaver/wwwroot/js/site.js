// Chat scrolling functions
window.scrollToBottom = (element) => {
    // Try using the element directly first
    if (element && element.scrollTo) {
        element.scrollTop = element.scrollHeight;
        return;
    }
    
    // Fallback to using ID
    const container = document.getElementById('conversationContainer');
    if (container) {
        container.scrollTop = container.scrollHeight;
    }
};

window.getScrollInfo = (element) => {
    // Try element first, then fallback to ID
    const target = element || document.getElementById('conversationContainer');
    if (target) {
        return [
            target.scrollTop,
            target.scrollHeight,
            target.clientHeight
        ];
    }
    return [0, 0, 0];
};