function setTextNode(key, value) {
  let textNode = keyholes[key];
  textNode.nodeValue = value;
}

function setAttribute(key, value) {
  let attribute = keyholes[key];
  if (typeof value !== "boolean") {
    attribute.value = value;
  } else {
    let attrName = attribute.booleanAttribute;
    attribute.owner[attrName] = value;
  }
}

function replaceElement(key, value, transition) {
  let oldElement = keyholes[key];
  let newElement = document
    .createRange()
    .createContextualFragment(value)
    .children[0];
  keyholes[key] = newElement;
  registerKeys(newElement);

  // Transition not requested or not supported
  if (!document.startViewTransition || !transition) {
    oldElement.replaceWith(newElement);
    return;
  }
  
  // Animate this mutation
  document.startViewTransition(() => {
    oldElement.replaceWith(newElement);
  });
}

function moveElement() {

}

function addElement() {

}

function removeElement() {

}

function clientRpc(data) {
  let batch = JSON.parse(data);
  if (!Array.isArray(batch)) batch = [batch];
  batch.forEach((jsonRpcMessage) => {
    let func = globalThis;
    jsonRpcMessage.method.split(".").forEach((s) => (func = func[s]));
    func.apply(globalThis, jsonRpcMessage.params)
  });
}

function serverRpc(key, event, includeProperties) {
  if (event && !event.propagationID)
    event.propagationID = ++propagationID;

  if (includeProperties?.includes("preventDefault")) {
    event?.preventDefault();
    if (includeProperties === "preventDefault")
      includeProperties = null;
  }
  includeProperties = includeProperties?.split(",") ?? ALLOWED_EVENT_PROPERTIES;

  let message = JSON.stringify(
    { jsonrpc: "2.0", method: key, params: event }, 
    [ "jsonrpc", "method", "params", ...includeProperties, "propagationID" ]
  );
  ws.send(message);
}

function registerKeys(parentNode) {
  let comments = document.evaluate('//comment()', parentNode, null, XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, null);
  for (let i = comments.snapshotLength - 1; i >= 0; i--) {
    let comment = comments.snapshotItem(i);
    let key = comment.textContent;
    if (key.startsWith('key')) {
      let keyhole = comment.previousSibling;
      if (keyhole.nodeType === Node.COMMENT_NODE) {
        // Text node was missing because keyhole value was ""
        // e.g. `<!----><!--key123-->`
        keyhole = keyhole.parentNode.insertBefore(document.createTextNode(""), comment);
      }
      keyholes[key] = keyhole;
    }
    comment.parentElement.removeChild(comment);
  }
  
  let attrs = document.evaluate('//*/attribute::*', parentNode, null, XPathResult.UNORDERED_NODE_SNAPSHOT_TYPE, null);
  for (let i = 0; i < attrs.snapshotLength; i++) {
    let attr = attrs.snapshotItem(i);
    let key = attr.name;
    if (key.startsWith('key')) {
      if (attr.value === "") {
        keyholes[key] = attrs.snapshotItem(i - 1);
      } else {
        keyholes[key] = { 
          nodeType: Node.ENTITY_REFERENCE_NODE, 
          booleanAttribute: attr.value, 
          owner: attr.ownerElement
        };
      }
      attr.ownerElement.removeAttribute(attr.name)
    }
  }
}

let keyholes = {};
registerKeys(document);

let propagationID = 0;
let ws = new WebSocket(`//${location.host}${location.pathname}`);
ws.onopen = console.debug;
ws.onclose = console.debug;
ws.onerror = console.error;
ws.onmessage = (e) => clientRpc(e.data);

const ALLOWED_EVENT_PROPERTIES = [ "absolute", "acceleration", "accelerationIncludingGravity", "alpha", "altitudeAngle", "altKey", "animationName", "azimuthAngle", "beta", "bubbles", "button", "buttons", "cancelable", "changedTouches", "clientX", "clientY", "code", "colNo", "composed", "ctrlKey", "currentTarget", "data", "dataTransfer", "defaultPrevented", "deltaMode", "deltaX", "deltaY", "deltaZ", "detail", "elapsedTime", "error", "eventPhase", "fileName", "gamma", "height", "inputType", "interval", "isComposing", "isPrimary", "isTrusted", "key", "length", "lengthComputable", "lineNo", "loaded", "location", "message", "metaKey", "movementX", "movementY", "newState", "newUrl", "offsetX", "offsetY", "oldState", "oldUrl", "pageX", "pageY", "persisted", "pointerID", "pointerType", "pressure", "propertyName", "pseudoElement", "relatedTarget", "repeat", "rotationRate", "screenX", "screenY", "shiftKey", "skipped", "submitter", "tangentialPressure", "target", "targetTouches", "timeStamp", "tiltX", "tiltY", "total", "touches", "twist", "type", "width", "x", "y", "id", "name", "value", "checked" ];
