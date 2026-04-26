import {
  AbsoluteFill, interpolate, spring,
  useCurrentFrame, useVideoConfig, Sequence,
} from "remotion";

const C = {
  primary: "#4f9ef8", success: "#38d9a9", warning: "#f7c948",
  info: "#4fc3f7", danger: "#f06595", bg: "#f0f4fb",
  card: "#ffffff", text: "#2b3674", muted: "#a3aed0", border: "#e8edf7",
};

// ── SVG ikonlar ───────────────────────────────────────────────────────────
const IMenu2 = ({ s = 18, c = "#fff" }) => (
  <svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke={c} strokeWidth="2.2" strokeLinecap="round">
    <line x1="3" y1="6" x2="21" y2="6"/><line x1="3" y1="12" x2="21" y2="12"/><line x1="3" y1="18" x2="21" y2="18"/>
  </svg>
);
const IUtensils = ({ s = 24, c = C.info }) => (
  <svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke={c} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
    <path d="M3 2v7c0 2.2 1.8 4 4 4s4-1.8 4-4V2"/><path d="M7 2v20"/>
    <path d="M21 15V2a5 5 0 0 0-5 5v6h3v7"/>
  </svg>
);
const ITag = ({ s = 22, c = C.warning }) => (
  <svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke={c} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
    <path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z"/>
    <line x1="7" y1="7" x2="7.01" y2="7" strokeWidth="2.5"/>
  </svg>
);
const IPlus = ({ s = 14, c = "#fff" }) => (
  <svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke={c} strokeWidth="2.5" strokeLinecap="round">
    <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
  </svg>
);
const IImage = ({ s = 28, c = C.muted }) => (
  <svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke={c} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="3" width="18" height="18" rx="2"/>
    <circle cx="8.5" cy="8.5" r="1.5"/><polyline points="21 15 16 10 5 21"/>
  </svg>
);
const ISearch = ({ s = 14, c = C.muted }) => (
  <svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke={c} strokeWidth="2" strokeLinecap="round">
    <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
  </svg>
);
const ICheck = ({ s = 12, c = "#fff" }) => (
  <svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke={c} strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="20 6 9 17 4 12"/>
  </svg>
);

// ── Yardımcılar ───────────────────────────────────────────────────────────
function FadeUp({ children, delay = 0, style }: { children: React.ReactNode; delay?: number; style?: React.CSSProperties }) {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  const p = spring({ frame: frame - delay, fps, config: { damping: 16, stiffness: 80 } });
  return (
    <div style={{ opacity: interpolate(p, [0, 1], [0, 1]), transform: `translateY(${interpolate(p, [0, 1], [20, 0])}px)`, ...style }}>
      {children}
    </div>
  );
}

function TypedText({ text, startFrame, color = C.text }: { text: string; startFrame: number; color?: string }) {
  const frame = useCurrentFrame();
  const elapsed = Math.max(0, frame - startFrame);
  const chars = Math.min(text.length, Math.floor(elapsed * 1.8));
  return (
    <span style={{ color }}>
      {text.slice(0, chars)}
      {chars < text.length && <span style={{ borderRight: `2px solid ${C.primary}`, marginLeft: 1, opacity: Math.floor(elapsed / 8) % 2 === 0 ? 1 : 0 }} />}
    </span>
  );
}


// ── SAHNE 1: Menü genel bakış ─────────────────────────────────────────────
const CATS = ["Başlangıçlar", "Ana Yemekler", "Tatlılar", "İçecekler", "Salatalar"];
const CAT_COLORS = [C.primary, C.success, C.warning, C.info, C.danger];

function Scene1() {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  return (
    <AbsoluteFill style={{ padding: "28px 36px", background: C.bg }}>
      <FadeUp delay={0} style={{ marginBottom: 18 }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
            <div style={{ width: 44, height: 44, borderRadius: 12, background: `${C.info}18`, display: "flex", alignItems: "center", justifyContent: "center" }}>
              <IUtensils s={24} c={C.info} />
            </div>
            <div>
              <div style={{ fontWeight: 800, fontSize: 16, color: C.text }}>Ana Menü</div>
              <div style={{ fontSize: 12, color: C.muted }}>5 kategori · 23 ürün</div>
            </div>
          </div>
          <div style={{ background: `${C.success}18`, color: C.success, borderRadius: 8, padding: "6px 12px", fontSize: 12, fontWeight: 700 }}>Yayında</div>
        </div>
      </FadeUp>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 10 }}>
        {CATS.map((cat, i) => {
          const p = spring({ frame: frame - 8 - i * 10, fps, config: { damping: 14 } });
          const col = CAT_COLORS[i];
          return (
            <div key={cat} style={{
              background: C.card, borderRadius: 12, padding: "13px 16px",
              boxShadow: "0 2px 12px rgba(79,158,248,.08)",
              opacity: p, transform: `translateY(${interpolate(p, [0, 1], [16, 0])}px)`,
              display: "flex", alignItems: "center", gap: 10,
              borderLeft: `3px solid ${col}`,
            }}>
              <ITag s={16} c={col} />
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 700, fontSize: 13, color: C.text }}>{cat}</div>
                <div style={{ fontSize: 11, color: C.muted }}>{[5, 8, 3, 4, 3][i]} ürün</div>
              </div>
            </div>
          );
        })}
      </div>
    </AbsoluteFill>
  );
}

// ── SAHNE 2: Kategori kütüphanesi + ekleme ────────────────────────────────
const LIB_ITEMS = ["Pizzalar", "Çorbalar", "Izgara", "Makarnalar", "Salatalar", "Tatlılar"];
const ADDED = ["Başlangıçlar", "Ana Yemekler", "Tatlılar"];

function Scene2() {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  return (
    <AbsoluteFill style={{ padding: "28px 36px", background: C.bg }}>
      <FadeUp delay={0} style={{ marginBottom: 14 }}>
        <div style={{ fontWeight: 800, fontSize: 16, color: C.text }}>Kategori Yönetimi</div>
        <div style={{ fontSize: 12, color: C.muted }}>Kütüphaneden seç ve menüne ekle</div>
      </FadeUp>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
        {/* Sol: Arama */}
        <div style={{ background: C.card, borderRadius: 12, padding: "14px", boxShadow: "0 2px 12px rgba(79,158,248,.08)" }}>
          <div style={{ fontWeight: 700, fontSize: 12, color: C.muted, marginBottom: 10, textTransform: "uppercase", letterSpacing: 0.5 }}>Kategori Bul</div>
          <div style={{ border: `1.5px solid ${C.border}`, borderRadius: 8, padding: "7px 10px", display: "flex", alignItems: "center", gap: 7, marginBottom: 10, background: "#f8faff" }}>
            <ISearch s={13} c={C.muted} />
            <span style={{ fontSize: 12, color: C.muted }}>Ara...</span>
          </div>
          <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
            {LIB_ITEMS.map((item, i) => {
              const p = spring({ frame: frame - 6 - i * 7, fps, config: { damping: 14 } });
              return (
                <div key={item} style={{
                  display: "flex", alignItems: "center", justifyContent: "space-between",
                  padding: "6px 8px", borderRadius: 8, border: `1px solid ${C.border}`,
                  opacity: p, transform: `translateX(${interpolate(p, [0, 1], [-10, 0])}px)`,
                }}>
                  <span style={{ fontSize: 12, fontWeight: 600, color: C.text }}>{item}</span>
                  <div style={{ width: 22, height: 22, borderRadius: 6, background: C.primary, display: "flex", alignItems: "center", justifyContent: "center" }}>
                    <IPlus s={11} c="#fff" />
                  </div>
                </div>
              );
            })}
          </div>
        </div>
        {/* Sağ: Eklenenler */}
        <div style={{ background: C.card, borderRadius: 12, padding: "14px", boxShadow: "0 2px 12px rgba(79,158,248,.08)" }}>
          <div style={{ fontWeight: 700, fontSize: 12, color: C.muted, marginBottom: 10, textTransform: "uppercase", letterSpacing: 0.5 }}>Menüm</div>
          <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
            {ADDED.map((cat, i) => {
              const p = spring({ frame: frame - 20 - i * 12, fps, config: { damping: 14 } });
              return (
                <div key={cat} style={{
                  background: `${C.warning}18`, border: `2px solid ${C.warning}`,
                  borderRadius: 9, padding: "8px 10px",
                  display: "flex", alignItems: "center", gap: 8,
                  opacity: p, transform: `scale(${interpolate(p, [0, 1], [0.8, 1])})`,
                }}>
                  <div style={{ width: 6, height: 6, borderRadius: "50%", background: C.warning, flexShrink: 0 }} />
                  <span style={{ flex: 1, fontSize: 12, fontWeight: 700, color: C.text }}>{cat}</span>
                  <div style={{ width: 18, height: 18, borderRadius: 5, background: C.danger, display: "flex", alignItems: "center", justifyContent: "center" }}>
                    <svg width="8" height="8" viewBox="0 0 24 24" fill="none" stroke="#fff" strokeWidth="3" strokeLinecap="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </AbsoluteFill>
  );
}

// ── SAHNE 3: Ürün listesi ─────────────────────────────────────────────────
const PRODUCTS = [
  { name: "Humus", desc: "Klasik nohut ezmesi", price: "₺95", active: true },
  { name: "Sigara Böreği", desc: "Çıtır peynirli börek", price: "₺75", active: true },
  { name: "Mercimek Çorbası", desc: "Ev yapımı", price: "₺65", active: true },
  { name: "Kalamar", desc: "Izgara veya kızartma", price: "₺120", active: false },
];

function Scene3() {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  return (
    <AbsoluteFill style={{ padding: "28px 36px", background: C.bg }}>
      <FadeUp delay={0} style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 16 }}>
        <div>
          <div style={{ fontWeight: 800, fontSize: 16, color: C.text }}>Başlangıçlar</div>
          <div style={{ fontSize: 12, color: C.muted }}>4 ürün</div>
        </div>
        <div style={{ background: C.primary, color: "#fff", borderRadius: 8, padding: "7px 12px", fontSize: 12, fontWeight: 700, display: "flex", alignItems: "center", gap: 6 }}>
          <IPlus s={11} c="#fff" /> Ürün Ekle
        </div>
      </FadeUp>
      <div style={{ background: C.card, borderRadius: 14, overflow: "hidden", boxShadow: "0 2px 16px rgba(79,158,248,.1)" }}>
        {PRODUCTS.map((p, i) => {
          const sp = spring({ frame: frame - 6 - i * 10, fps, config: { damping: 14 } });
          return (
            <div key={p.name} style={{
              display: "flex", alignItems: "center", gap: 14, padding: "13px 18px",
              borderBottom: i < PRODUCTS.length - 1 ? `1px solid ${C.border}` : "none",
              opacity: sp, transform: `translateX(${interpolate(sp, [0, 1], [-12, 0])}px)`,
            }}>
              <div style={{ width: 40, height: 40, borderRadius: 10, background: C.bg, display: "flex", alignItems: "center", justifyContent: "center", flexShrink: 0 }}>
                <IImage s={20} c={C.muted} />
              </div>
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 700, fontSize: 13, color: C.text }}>{p.name}</div>
                <div style={{ fontSize: 11, color: C.muted }}>{p.desc}</div>
              </div>
              <div style={{ fontWeight: 800, fontSize: 15, color: C.primary }}>{p.price}</div>
              <div style={{ background: p.active ? `${C.success}18` : `${C.danger}18`, color: p.active ? C.success : C.danger, borderRadius: 99, padding: "2px 8px", fontSize: 10, fontWeight: 700 }}>
                {p.active ? "Aktif" : "Pasif"}
              </div>
            </div>
          );
        })}
      </div>
    </AbsoluteFill>
  );
}

// ── SAHNE 4: Ürün ekleme formu ────────────────────────────────────────────
function Scene4() {
  const frame = useCurrentFrame();
  const saved = frame >= 120;
  return (
    <AbsoluteFill style={{ padding: "28px 36px", background: C.bg }}>
      <FadeUp delay={0} style={{ marginBottom: 16 }}>
        <div style={{ fontWeight: 800, fontSize: 16, color: C.text }}>Yeni Ürün Ekle</div>
        <div style={{ fontSize: 12, color: C.muted }}>Başlangıçlar kategorisi</div>
      </FadeUp>
      <div style={{ background: C.card, borderRadius: 14, padding: "20px 22px", boxShadow: "0 2px 16px rgba(79,158,248,.1)" }}>
        {/* Ürün adı */}
        <FadeUp delay={8} style={{ marginBottom: 14 }}>
          <div style={{ fontSize: 12, fontWeight: 600, color: C.muted, marginBottom: 6 }}>Ürün Adı</div>
          <div style={{ border: `2px solid ${C.primary}`, borderRadius: 9, padding: "9px 13px", fontSize: 14, fontWeight: 600, color: C.text }}>
            <TypedText text="Humus" startFrame={15} />
          </div>
        </FadeUp>
        {/* Fiyat + Açıklama */}
        <FadeUp delay={16} style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12, marginBottom: 14 }}>
          <div>
            <div style={{ fontSize: 12, fontWeight: 600, color: C.muted, marginBottom: 6 }}>Fiyat</div>
            <div style={{ border: `2px solid ${C.success}`, borderRadius: 9, padding: "9px 13px", fontSize: 14, fontWeight: 600, color: C.text }}>
              <TypedText text="₺95" startFrame={30} color={C.success} />
            </div>
          </div>
          <div>
            <div style={{ fontSize: 12, fontWeight: 600, color: C.muted, marginBottom: 6 }}>Durum</div>
            <div style={{ border: `2px solid ${C.border}`, borderRadius: 9, padding: "9px 13px", fontSize: 14, color: C.muted, display: "flex", alignItems: "center", gap: 8 }}>
              <div style={{ width: 28, height: 16, borderRadius: 99, background: C.success, display: "flex", alignItems: "center", paddingRight: 2, justifyContent: "flex-end" }}>
                <div style={{ width: 12, height: 12, borderRadius: "50%", background: "#fff" }} />
              </div>
              <span style={{ fontSize: 12, color: C.success, fontWeight: 600 }}>Aktif</span>
            </div>
          </div>
        </FadeUp>
        {/* Fotoğraf */}
        <FadeUp delay={24} style={{ marginBottom: 18 }}>
          <div style={{ fontSize: 12, fontWeight: 600, color: C.muted, marginBottom: 6 }}>Fotoğraf</div>
          <div style={{
            border: `2px dashed ${C.border}`, borderRadius: 10, padding: "18px",
            display: "flex", flexDirection: "column", alignItems: "center", gap: 6,
            background: "#f8faff",
          }}>
            <IImage s={30} c={C.muted} />
            <div style={{ fontSize: 12, color: C.muted }}>Fotoğraf yükle veya sürükle</div>
          </div>
        </FadeUp>
        {/* Kaydet butonu */}
        <FadeUp delay={32}>
          <div style={{
            background: saved ? C.success : C.primary,
            color: "#fff", borderRadius: 10, padding: "13px 20px",
            fontWeight: 700, fontSize: 15, textAlign: "center",
            transform: saved ? "scale(0.97)" : "scale(1)",
            boxShadow: saved ? "none" : `0 4px 18px ${C.primary}55`,
            display: "flex", alignItems: "center", justifyContent: "center", gap: 8,
          }}>
            {saved ? <ICheck s={14} c="#fff" /> : null}
            {saved ? "Kaydedildi!" : "Ürünü Kaydet"}
            {!saved && (
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="#fff" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                <line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/>
              </svg>
            )}
          </div>
        </FadeUp>
      </div>
    </AbsoluteFill>
  );
}

// ── Admin sidebar ─────────────────────────────────────────────────────────
function Sidebar() {
  const items = [
    { label: "Panel", active: false },
    { label: "Menüler", active: true, color: C.info },
    { label: "Kategoriler", active: false },
    { label: "Ürünler", active: false },
  ];
  return (
    <div style={{ width: 130, background: "#1e2b4a", display: "flex", flexDirection: "column", padding: "16px 0", gap: 2, flexShrink: 0 }}>
      <div style={{ padding: "0 14px 16px", display: "flex", alignItems: "center", gap: 8, borderBottom: "1px solid rgba(255,255,255,.08)", marginBottom: 6 }}>
        <div style={{ width: 28, height: 28, borderRadius: 8, background: C.info, display: "flex", alignItems: "center", justifyContent: "center" }}>
          <IMenu2 s={14} c="#fff" />
        </div>
        <span style={{ fontSize: 12, fontWeight: 800, color: "#fff" }}>TümMenu</span>
      </div>
      {items.map(({ label, active, color }) => (
        <div key={label} style={{
          padding: "7px 14px", display: "flex", alignItems: "center", gap: 8,
          background: active ? `${color ?? C.primary}22` : "transparent",
          borderLeft: active ? `3px solid ${color ?? C.primary}` : "3px solid transparent",
        }}>
          <span style={{ fontSize: 11, color: active ? (color ?? C.primary) : "rgba(255,255,255,.45)", fontWeight: active ? 700 : 400 }}>{label}</span>
        </div>
      ))}
    </div>
  );
}

function TitleBar() {
  return (
    <div style={{ background: "#dde4ef", padding: "10px 18px", display: "flex", alignItems: "center", gap: 8, flexShrink: 0, borderBottom: "1px solid #ccd4e8" }}>
      <div style={{ width: 13, height: 13, borderRadius: "50%", background: "#ff5f57" }} />
      <div style={{ width: 13, height: 13, borderRadius: "50%", background: "#ffbd2e" }} />
      <div style={{ width: 13, height: 13, borderRadius: "50%", background: "#28c840" }} />
      <div style={{ flex: 1, background: "#f0f4fb", borderRadius: 7, padding: "4px 14px", fontSize: 12, color: C.muted, marginLeft: 10, display: "flex", alignItems: "center", gap: 6 }}>
        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke={C.muted} strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
        tummenu.com/admin/menuler
      </div>
    </div>
  );
}

// ── ANA KOMPOZİSYON ──────────────────────────────────────────────────────
export const MenuProductVideo: React.FC = () => {
  return (
    <AbsoluteFill style={{ background: "#e8edf7", display: "flex", flexDirection: "column" }}>
      <TitleBar />
      <div style={{ flex: 1, display: "flex", overflow: "hidden" }}>
        <Sidebar />
        <div style={{ flex: 1, position: "relative", overflow: "hidden" }}>
          <Sequence from={0} durationInFrames={110}><Scene1 /></Sequence>
          <Sequence from={110} durationInFrames={140}><Scene2 /></Sequence>
          <Sequence from={250} durationInFrames={140}><Scene3 /></Sequence>
          <Sequence from={390} durationInFrames={150}><Scene4 /></Sequence>
        </div>
      </div>
    </AbsoluteFill>
  );
};
