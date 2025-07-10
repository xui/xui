function rpc(key, event, incl) {
  if (incl?.includes("preventDefault")) {
    event.preventDefault();
    incl = incl.replace("preventDefault,", "");
    if (incl == "null") incl = null;
  }
  ws.send(JSON.stringify({
    jsonrpc: "2.0",
    method: key,
    params: event ? trimEvent(event, incl) : undefined,
  }));
}

function mutate(key, value) {
  let node = ui[key];
  if (!node) return console.error(`Keyhole ${key} is missing`);
  switch (node.nodeType) {
    case Node.TEXT_NODE:              node.nodeValue = value; break;
    case Node.ATTRIBUTE_NODE:         node.value = value; break;
    case Node.ELEMENT_NODE:           node.outerHTML = value; break;
    case Node.ENTITY_REFERENCE_NODE:  node.owner[node.booleanAttribute] = value; break;
  }
}

let eventID = 0;
function trimEvent(e, incl) {
  const allowList = incl?.split(",") ?? eventKeys;
  const json = {};
  for (let k in e) {
    let v = e[k];
    if (v != null && allowList.includes(k)) {
      if (v instanceof EventTarget) {
        json[k] = {};
        for (let k2 in v) {
          let v2 = v[k2];
          if (eventTargetKeys.includes(k2) && v2 !== "") json[k][k2] = v2;
        }
      } else {
        json[k] = v;
      }
    }
  }
  if (!e._id) e._id = ++eventID;
  json._id = e._id;
  return json;
}

function replaceNode(node, content) {
  const regScript = node.nextSibling;
  node.outerHTML = content;
  node = regScript.previousSibling;
  reRegister(regScript);
  for (let s of node.getElementsByTagName("script")) {
    reRegister(s);
  }
}

function reRegister(node) {
  if (node.tagName == "SCRIPT") {
    const s = document.createElement("script");
    s.textContent = node.textContent;
    node.replaceWith(s);
  }
}

function findRpcFunction(name) {
  let f = globalThis;
  name.split(".").forEach((s) => (f = f[s]));
  return f;
}

function clientRpc(message) {
  let json = JSON.parse(message);
  if (!Array.isArray(json)) json = [json];
  json.forEach((message) =>
    findRpcFunction(message.method).apply(globalThis, message.params)
  );
}

function bootstrap() {
  const l = location;
  const p = l.protocol.replace("http", "ws");
  const ws = new WebSocket(`${p}//${l.host}${l.pathname}`);
  globalThis["ws"] = ws;
  ws.onopen = console.debug;
  ws.onclose = console.debug;
  ws.onerror = console.error;
  ws.onmessage = (e) => clientRpc(e.data);

  globalThis["ui"] = {};
  for (let k in ui) {
    let n = ui[k];
    if (n.nodeType == 8) {
      let t = document.createTextNode("");
      n.parentNode.insertBefore(t, n.nextSibling);
      ui[k] = t;
    }
  }

  let comments = document.evaluate('//comment()', document, null, 0, null);
  let node = comments.iterateNext();
  while (node) {
    let key = node.textContent;
    if (key.startsWith('key')) {
      ui[key] = node.previousSibling;
    }
    node = comments.iterateNext();
  }

  let attrs = document.evaluate('//*/attribute::*', document, null, 7, null);
  for (i = 0; i < attrs.snapshotLength; i++) {
    let attr = attrs.snapshotItem(i);
    let key = attr.name;
    if (key.startsWith('key')) {
      if (attr.value === "") {
        ui[key] = attrs.snapshotItem(i - 1);
      } else {
        ui[key] = { 
          nodeType: Node.ENTITY_REFERENCE_NODE, 
          booleanAttribute: attr.value, 
          owner: attr.ownerElement
        };
      }
    }
  }

  // TODO: What is the performance cost of manipulating the DOM so many times onload?
  // Clean up the many <script> and <!-- --> nodes.
  while (document.scripts.length > 0) {
    const s = document.scripts[0];
    const c = s.previousSibling.previousSibling;
    if (c.nodeType == Node.COMMENT_NODE) {
      c.parentNode.removeChild(c);
    }
    s.parentNode.removeChild(s);
  }
  for (i = 0; i < attrs.snapshotLength; i++) {
    let attr = attrs.snapshotItem(i);
    if (attr.name.startsWith('key')) {
      attr.ownerElement.removeAttribute(attr.name)
    }
  }
}

const eventTargetKeys = ["id", "name", "type", "value", "checked"];
const eventKeys = [ "absolute", "acceleration", "accelerationIncludingGravity", "alpha", "altitudeAngle", "altKey", "animationName", "azimuthAngle", "beta", "bubbles", "button", "buttons", "cancelable", "changedTouches", "clientX", "clientY", "code", "colNo", "composed", "ctrlKey", "currentTarget", "data", "dataTransfer", "defaultPrevented", "deltaMode", "deltaX", "deltaY", "deltaZ", "detail", "elapsedTime", "error", "eventPhase", "fileName", "gamma", "height", "inputType", "interval", "isComposing", "isPrimary", "isTrusted", "key", "length", "lengthComputable", "lineNo", "loaded", "location", "message", "metaKey", "movementX", "movementY", "newState", "newUrl", "offsetX", "offsetY", "oldState", "oldUrl", "pageX", "pageY", "persisted", "pointerID", "pointerType", "pressure", "propertyName", "pseudoElement", "relatedTarget", "repeat", "rotationRate", "screenX", "screenY", "shiftKey", "skipped", "submitter", "tangentialPressure", "target", "targetTouches", "timeStamp", "tiltX", "tiltY", "total", "touches", "twist", "type", "width", "x", "y" ];

bootstrap();
