// ── Terminal ─────────────────────────────────────────────────────────────────

const term = new Terminal({
    cursorBlink: true,
    cursorStyle: 'block',
    theme: {
        background: '#111111',
        foreground: '#cccccc',
        cursor: '#cccccc',
    },
});

const fitAddon = new FitAddon.FitAddon();
term.loadAddon(fitAddon);
term.open(document.getElementById('terminal'));
term.onResize(() => term.scrollToBottom());

new ResizeObserver(() => fitAddon.fit()).observe(document.getElementById('terminal-container'));

function writePrompt(prompt) {
    term.write(prompt ?? '>');
}

writePrompt('[no caster] >');


// ── Input handling ────────────────────────────────────────────────────────────

let input        = '';
let cursorOffset = 0;
const history    = [];
let historyIndex = -1;

const ESC = '\x1b[';
const CURSOR_LEFT  = ESC + '1D';
const CURSOR_RIGHT = ESC + '1C';

const moveCursorLeft = n => n === 0 ? '' : `${ESC}${n}D`;

const clearInput = () =>
    moveCursorLeft(input.length - cursorOffset) +
    ' '.repeat(input.length) +
    moveCursorLeft(input.length);

function handleTerminalKey(key, domEvent) {
    switch (domEvent.key) {
        case 'Backspace': {
            const before = input.slice(0, input.length - cursorOffset);
            if (!before.length) return;
            const after = input.slice(input.length - cursorOffset);
            input = before.slice(0, -1) + after;
            term.write(
                CURSOR_LEFT +
                (after[0] ?? '') +
                after.slice(1) +
                ' ' +
                moveCursorLeft(cursorOffset + 1));
            break;
        }
        case 'Delete': {
            const after = input.slice(input.length - cursorOffset);
            if (!after.length) return;
            input = input.slice(0, input.length - cursorOffset) + after.slice(1);
            cursorOffset--;
            term.write(after.slice(1) + ' ' + moveCursorLeft(cursorOffset + 1));
            break;
        }
        case 'ArrowLeft':
            if (input.length - cursorOffset > 0) { cursorOffset++; term.write(CURSOR_LEFT); }
            break;
        case 'ArrowRight':
            if (cursorOffset > 0) { cursorOffset--; term.write(CURSOR_RIGHT); }
            break;
        case 'ArrowUp':
            if (historyIndex < history.length - 1) {
                historyIndex++;
                term.write(clearInput());
                input = history[history.length - 1 - historyIndex];
                cursorOffset = 0;
                term.write(input);
            }
            break;
        case 'ArrowDown':
            if (historyIndex > 0) {
                historyIndex--;
                term.write(clearInput());
                input = history[history.length - 1 - historyIndex];
                cursorOffset = 0;
                term.write(input);
            } else if (historyIndex === 0) {
                historyIndex = -1;
                term.write(clearInput());
                input = '';
                cursorOffset = 0;
            }
            break;
        case 'Enter':
            submitInput();
            break;
        default:
            if (key.charCodeAt(0) > 31 && key.charCodeAt(0) < 127) {
                const before = input.slice(0, input.length - cursorOffset);
                const after  = input.slice(input.length - cursorOffset);
                input = before + key + after;
                term.write(key + after + moveCursorLeft(cursorOffset));
            }
    }
}

term.onKey(({ key, domEvent }) => handleTerminalKey(key, domEvent));

const REDIRECT_KEYS = new Set(['Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Enter']);

document.addEventListener('keydown', e => {
    if (document.activeElement?.closest('#terminal-container')) return;
    if (e.key.length !== 1 && !REDIRECT_KEYS.has(e.key)) return;
    e.preventDefault();
    term.focus();
    handleTerminalKey(e.key, e);
});

async function submitInput() {
    const cmd = input;
    input        = '';
    cursorOffset = 0;
    historyIndex = -1;
    if (cmd.length) history.push(cmd);

    term.write('\r\n');
    await sendCommand(cmd);
}


// ── API ───────────────────────────────────────────────────────────────────────

async function sendCommand(cmd) {
    await fetch('/command', {
        method:  'POST',
        headers: { 'Content-Type': 'application/json' },
        body:    JSON.stringify(cmd),
    });
}


// ── SSE ───────────────────────────────────────────────────────────────────────

let latestEntities = null;
let camera = null;
let lastRenderedEntities = null;
let cameraDirty = false;

const eventSource = new EventSource('/events');
eventSource.onmessage = (e) => {
    const result = JSON.parse(e.data);
    for (const line of (result.text ?? [])) {
        if (line) term.writeln(line);
    }
    if (result.text?.length && result.prompt) {
        writePrompt(result.prompt);
    }
    if (result.text?.length || result.prompt) {
        term.scrollToBottom();
    }
    if (result.entities?.length) {
        latestEntities = result.entities;
    }
    updateBars(result.bars);
};

(function renderLoop() {
    if (latestEntities) {
        updateCanvas(latestEntities);
        latestEntities = null;
        cameraDirty = false;
    } else if (cameraDirty && lastRenderedEntities) {
        updateCanvas(lastRenderedEntities);
        cameraDirty = false;
    }
    requestAnimationFrame(renderLoop);
})();


// ── Caster bars ───────────────────────────────────────────────────────────────

function updateBars(bars) {
    updateBar('hp',    bars?.currentHitPoints, bars?.maxHitPoints);
    updateBar('power', bars?.currentPower,     bars?.maxPower);
}

function updateBar(name, current, max) {
    const pct = (current != null && max > 0) ? (current / max * 100) : 0;
    document.getElementById(`${name}-fill`).style.width = pct + '%';
    document.getElementById(`${name}-value`).textContent = current != null ? `${current}/${max}` : '';
}


// ── Canvas ────────────────────────────────────────────────────────────────────

const SVG_NS = 'http://www.w3.org/2000/svg';
const svg    = document.getElementById('world-canvas');

const FLAGS_HAS_LIFE = 1;
const FLAGS_HAS_AGENCY = 2;
const FLAGS_IS_TRANSLUCENT = 4;

function applyCamera() {
    svg.setAttribute('viewBox',
        `${camera.cx - camera.w / 2} ${camera.cy - camera.h / 2} ${camera.w} ${camera.h}`);
}

function entityClass(entity) {
    let cls;
    if ((entity.flags & FLAGS_HAS_LIFE) && (entity.flags & FLAGS_HAS_AGENCY)) cls = 'entity entity-creature';
    else if (entity.flags & FLAGS_HAS_LIFE) cls = 'entity entity-life';
    else if (entity.flags & FLAGS_HAS_AGENCY) cls = 'entity entity-agency';
    else cls = 'entity entity-object';
    if (entity.isCaster) cls += ' entity-caster';
    if (entity.flags & FLAGS_IS_TRANSLUCENT) cls += ' entity-translucent';
    if (entity.isIndicateTarget) cls += ' entity-indicate-target';
    return cls;
}

function svgEl(tag, attrs) {
    const el = document.createElementNS(SVG_NS, tag);
    for (const [k, v] of Object.entries(attrs)) el.setAttribute(k, v);
    return el;
}

function updateCanvas(entities) {
    lastRenderedEntities = entities;

    while (svg.firstChild) {
        svg.removeChild(svg.firstChild);
    }

    if (!entities.length) {
        svg.setAttribute('viewBox', '0 0 1000 1000');
        const text = svgEl('text', {
            x: 500,
            y: 500,
            'dominant-baseline': 'middle',
            'text-anchor': 'middle',
            class: 'canvas-placeholder',
        });
        text.textContent = 'no world loaded';
        svg.appendChild(text);
        return;
    }

    if (!camera) {
        let minX = Infinity;
        let minY = Infinity;
        let maxX = -Infinity;
        let maxY = -Infinity;

        for (const e of entities) {
            const hw = e.width / 2;
            const hh = e.height / 2;
            const cos = Math.abs(Math.cos(e.facing));
            const sin = Math.abs(Math.sin(e.facing));
            const extX = hw * cos + hh * sin;
            const extY = hw * sin + hh * cos;
            minX = Math.min(minX, e.x - extX);
            minY = Math.min(minY, -e.y - extY);
            maxX = Math.max(maxX, e.x + extX);
            maxY = Math.max(maxY, -e.y + extY);
        }

        const pad = 100;
        const vbWidth = maxX - minX + pad * 2;
        const vbHeight = maxY - minY + pad * 2;
        camera = {
            cx: (minX + maxX) / 2,
            cy: (minY + maxY) / 2,
            w: vbWidth,
            h: vbHeight,
        };
    }

    applyCamera();

    const svgRect = svg.getBoundingClientRect();
    const screenScale = Math.min(svgRect.width / camera.w, svgRect.height / camera.h);
    const labelSize = 13 / screenScale;

    for (const e of entities) {
        const g = svgEl('g', e.isCaster ? { class: 'caster-group' } : {});
        const angleDeg = e.facing * 180 / Math.PI;
        const hw = e.width / 2;
        const hh = e.height / 2;

        g.appendChild(svgEl('rect', {
            x: e.x - hw,
            y: -e.y - hh,
            width: e.width,
            height: e.height,
            class: entityClass(e),
            transform: `rotate(${-angleDeg}, ${e.x}, ${-e.y})`,
        }));

        const spread = Math.min(hw, hh) * 0.3;
        const depth = Math.min(hw, hh) * 0.3;

        const tx = e.x + hw;
        const ty = -e.y;
        const ax = e.x + hw - depth;
        const ay1 = -e.y - spread;
        const ay2 = -e.y + spread;

        const facingClass = (e.flags & FLAGS_HAS_AGENCY)
            ? 'entity-facing'
            : 'entity-facing entity-facing-hover';

        g.appendChild(svgEl('path', {
            d: `M ${ax},${ay1} L ${tx},${ty} L ${ax},${ay2}`,
            class: facingClass,
            transform: `rotate(${-angleDeg}, ${e.x}, ${-e.y})`,
        }));

        const sinF = Math.sin(e.facing);
        const cosF = Math.cos(e.facing);

        const tipRY = -e.y - hw * sinF;
        const arm1RY = -e.y - (hw - depth) * sinF - spread * cosF;
        const arm2RY = -e.y - (hw - depth) * sinF + spread * cosF;

        const chevronTop = Math.min(tipRY, arm1RY, arm2RY);
        const chevronBottom = Math.max(tipRY, arm1RY, arm2RY);

        const halfLabelH = labelSize * 0.5;
        const labelY = (-e.y - halfLabelH < chevronBottom && -e.y + halfLabelH > chevronTop)
            ? chevronBottom + halfLabelH + (labelSize * 0.2)
            : -e.y;

        const label = svgEl('text', {
            x: e.x,
            y: labelY,
            'dominant-baseline': 'middle',
            'text-anchor': 'middle',
            'font-size': labelSize,
            class: (e.flags & FLAGS_HAS_AGENCY) ? 'entity-label' : 'entity-label entity-label-hover',
        });
        label.textContent = e.label;
        g.appendChild(label);

        if (e.indicateEndX != null) {
            appendVector(g, e.x, -e.y, e.indicateEndX, -e.indicateEndY, 'entity-indicate', 150);
        }
        if (e.pointingEndX != null) {
            appendVector(g, e.x, -e.y, e.pointingEndX, -e.pointingEndY, 'entity-direction', 250);
        }

        svg.appendChild(g);
    }
}

function appendVector(parent, sx, sy, ex, ey, cls, arrowLen) {
    const dx = ex - sx;
    const dy = ey - sy;
    const len = Math.sqrt(dx * dx + dy * dy);

    if (len <= 0) return;

    const ux = dx / len;
    const uy = dy / len;
    const px = -uy;
    const py = ux;
    const halfArrow = arrowLen * 0.5;

    parent.appendChild(svgEl('line', {
        x1: sx,
        y1: sy,
        x2: ex,
        y2: ey,
        class: cls,
    }));

    parent.appendChild(svgEl('path', {
        d: `M ${ex - ux * arrowLen + px * halfArrow},${ey - uy * arrowLen + py * halfArrow}` +
            ` L ${ex},${ey}` +
            ` L ${ex - ux * arrowLen - px * halfArrow},${ey - uy * arrowLen - py * halfArrow}`,
        class: cls,
    }));
}


// ── Zoom ──────────────────────────────────────────────────────────────────────

svg.addEventListener('wheel', e => {
    e.preventDefault();
    if (!camera) return;

    const pt = svg.createSVGPoint();
    pt.x = e.clientX;
    pt.y = e.clientY;
    const svgPt = pt.matrixTransform(svg.getScreenCTM().inverse());

    const factor = e.deltaY < 0 ? 1 / 1.15 : 1.15;
    camera.cx = svgPt.x + (camera.cx - svgPt.x) * factor;
    camera.cy = svgPt.y + (camera.cy - svgPt.y) * factor;
    camera.w *= factor;
    camera.h *= factor;
    cameraDirty = true;
}, { passive: false });


// ── Mode toggle ───────────────────────────────────────────────────────────────

let currentMode = null;

const modeButtons = {
    'pick-caster': document.getElementById('btn-pick-caster'),
    'move-caster': document.getElementById('btn-move-caster'),
    'point-at':    document.getElementById('btn-point-at'),
    'indicate':    document.getElementById('btn-indicate'),
};

function setMode(mode) {
    currentMode = mode;
    for (const [key, btn] of Object.entries(modeButtons)) {
        btn.classList.toggle('active', key === mode);
    }
}

for (const [mode, btn] of Object.entries(modeButtons)) {
    btn.addEventListener('click', () => {
        setMode(currentMode === mode ? null : mode);
    });
}

document.addEventListener('contextmenu', e => {
    if (!currentMode) return;
    e.preventDefault();
    setMode(null);
});

let svgMousedownX = 0, svgMousedownY = 0;
let panArmed = false;
let isPanning = false;
let panStartScreenX = 0, panStartScreenY = 0;
let panStartCamCX = 0, panStartCamCY = 0;
let panScale = 1;

svg.addEventListener('mousedown', e => {
    svgMousedownX = e.clientX;
    svgMousedownY = e.clientY;
    if (!currentMode && camera) {
        panArmed = true;
        isPanning = false;
        panStartScreenX = e.clientX;
        panStartScreenY = e.clientY;
        panStartCamCX = camera.cx;
        panStartCamCY = camera.cy;
        const svgRect = svg.getBoundingClientRect();
        panScale = Math.min(svgRect.width / camera.w, svgRect.height / camera.h);
    }
});

svg.addEventListener('mouseup', async e => {
    if (isPanning) {
        isPanning = false;
        panArmed = false;
        return;
    }
    panArmed = false;
    if (!currentMode) return;

    const dx = e.clientX - svgMousedownX;
    const dy = e.clientY - svgMousedownY;
    if (Math.sqrt(dx * dx + dy * dy) > 5) return;

    const pt = svg.createSVGPoint();
    pt.x = e.clientX;
    pt.y = e.clientY;
    const svgPt = pt.matrixTransform(svg.getScreenCTM().inverse());

    const mode = currentMode;
    setMode(null);

    await sendModeClick(mode, svgPt.x, -svgPt.y);
});

async function sendModeClick(mode, x, y) {
    await fetch(`/${mode}`, {
        method:  'POST',
        headers: { 'Content-Type': 'application/json' },
        body:    JSON.stringify({ x, y }),
    });
}


// ── Divider drag ──────────────────────────────────────────────────────────────

const divider           = document.getElementById('divider');
const terminalContainer = document.getElementById('terminal-container');

let dragging           = false;
let dragStartY         = 0;
let dragStartHeight    = 0;

divider.addEventListener('mousedown', e => {
    dragging        = true;
    dragStartY      = e.clientY;
    dragStartHeight = terminalContainer.offsetHeight;
    document.body.style.cursor    = 'ns-resize';
    document.body.style.userSelect = 'none';
    e.preventDefault();
});

document.addEventListener('mousemove', e => {
    if (dragging) {
        const delta = dragStartY - e.clientY;
        const newHeight = Math.max(50, Math.min(window.innerHeight - 100, dragStartHeight + delta));
        terminalContainer.style.height = newHeight + 'px';
    }
    if (panArmed && e.buttons === 1) {
        const dx = e.clientX - panStartScreenX;
        const dy = e.clientY - panStartScreenY;
        if (!isPanning && Math.sqrt(dx * dx + dy * dy) > 5) {
            isPanning = true;
        }
        if (isPanning) {
            camera.cx = panStartCamCX - dx / panScale;
            camera.cy = panStartCamCY - dy / panScale;
            cameraDirty = true;
        }
    }
});

document.addEventListener('mouseup', () => {
    if (dragging) {
        dragging = false;
        document.body.style.cursor = '';
        document.body.style.userSelect = '';
    }
    panArmed = false;
    isPanning = false;
});
