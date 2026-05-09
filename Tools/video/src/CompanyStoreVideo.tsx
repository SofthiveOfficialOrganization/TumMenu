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
const IBuilding = ({ s = 24, c = C.primary }) => (
  <svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke={c} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="3" width="18" height="18" rx="2"/><path d="M3 9h18"/><path d="M9 21V9"/>
    <rect x="12" y="13" width="3" height="3"/><rect x="12" y="17" width="3" height="3"/>
  </svg>
);
const IStore = ({ s = 24, c = C.success }) => (
  <svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke={c} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
    <path d="M3 9l1-6h16l1 6"/><path d="M3 9a3 3 0 0 0 6 0 3 3 0 0 0 6 0 3 3 0 0 0 6 0"/>
    <path d="M5 21V9.5"/><path d="M19 21V9.5"/><rect x="8" y="14" width="8" height="7" rx="1"/>
  </svg>
);
const IQr = ({ s = 28, c = C.primary }) => (
  <svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke={c} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/>
    <rect x="3" y="14" width="7" height="7" rx="1"/>
    <rect x="5" y="5" width="3" height="3" fill={c} stroke="none"/>
    <rect x="16" y="5" width="3" height="3" fill={c} stroke="none"/>
    <rect x="5" y="16" width="3" height="3" fill={c} stroke="none"/>
    <path d="M17 14h4M14 17v4M17 17h4M17 21h4"/>
  </svg>
);
const IChart = ({ s = 24, c = C.primary }) => (
  <svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke={c} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
    <line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/>
    <line x1="6" y1="20" x2="6" y2="14"/><line x1="2" y1="20" x2="22" y2="20"/>
  </svg>
);
const IMenu = ({ s = 18, c = "#fff" }) => (
  <svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke={c} strokeWidth="2.2" strokeLinecap="round">
    <line x1="3" y1="6" x2="21" y2="6"/><line x1="3" y1="12" x2="21" y2="12"/><line x1="3" y1="18" x2="21" y2="18"/>
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

function Badge({ label, color }: { label: string; color: string }) {
  return (
    <div style={{ background: `${color}18`, color, borderRadius: 99, padding: "3px 10px", fontSize: 11, fontWeight: 700 }}>
      {label}
    </div>
  );
}

function Card({ children, delay = 0 }: { children: React.ReactNode; delay?: number }) {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  const p = spring({ frame: frame - delay, fps, config: { damping: 14 } });
  return (
    <div style={{
      background: C.card, borderRadius: 14, padding: "18px 20px",
      boxShadow: "0 2px 16px rgba(79,158,248,.1)",
      opacity: p, transform: `translateY(${interpolate(p, [0, 1], [18, 0])}px)`,
    }}>
      {children}
    </div>
  );
}

function PageHeader({ title, sub, btnLabel, color = C.primary }: { title: string; sub?: string; btnLabel: string; color?: string }) {
  return (
    <FadeUp delay={0} style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 18 }}>
      <div>
        <div style={{ fontSize: 19, fontWeight: 800, color: C.text }}>{title}</div>
        {sub && <div style={{ fontSize: 12, color: C.muted, marginTop: 1 }}>{sub}</div>}
      </div>
      <div style={{ background: color, color: "#fff", borderRadius: 8, padding: "7px 14px", fontSize: 12, fontWeight: 700 }}>
        {btnLabel}
      </div>
    </FadeUp>
  );
}

// ── SAHNE 1: Şirket listesi ────────────────────────────────────────────────
function Scene1() {
  return (
    <AbsoluteFill style={{ padding: "28px 36px", background: C.bg }}>
      <PageHeader title="Şirketlerim" sub="1 şirket" btnLabel="+ Şirket Ekle" />
      <Card delay={8}>
        <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 14 }}>
          <div style={{ width: 44, height: 44, borderRadius: 12, background: `${C.primary}18`, display: "flex", alignItems: "center", justifyContent: "center" }}>
            <IBuilding s={24} c={C.primary} />
          </div>
          <div style={{ flex: 1 }}>
            <div style={{ fontWeight: 800, fontSize: 15, color: C.text }}>Güzel Yemekler A.Ş.</div>
            <div style={{ fontSize: 12, color: C.muted }}>tummenu.com/guzel-yemekler</div>
          </div>
          <Badge label="Aktif" color={C.success} />
        </div>
        <div style={{ display: "flex", gap: 20, marginBottom: 16, paddingBottom: 14, borderBottom: `1px solid ${C.border}` }}>
          {[["2", "Şube"], ["1", "Menü"], ["23", "Ürün"]].map(([v, l]) => (
            <div key={l}>
              <div style={{ fontSize: 20, fontWeight: 800, color: C.primary }}>{v}</div>
              <div style={{ fontSize: 11, color: C.muted }}>{l}</div>
            </div>
          ))}
        </div>
        <div style={{ display: "flex", gap: 8 }}>
          <Badge label="Yönet" color={C.primary} />
          <Badge label="+ Şube Ekle" color={C.success} />
          <Badge label="Menüler" color={C.info} />
        </div>
      </Card>
    </AbsoluteFill>
  );
}

// ── SAHNE 2: Şube listesi ─────────────────────────────────────────────────
const STORES = [
  { name: "Merkez Şube", addr: "Beşiktaş, İstanbul", qr: 234, color: C.primary },
  { name: "Kadıköy Şube", addr: "Kadıköy, İstanbul", qr: 87, color: C.success },
];

function Scene2() {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  return (
    <AbsoluteFill style={{ padding: "28px 36px", background: C.bg }}>
      <PageHeader title="Şubelerim" sub="2 şube" btnLabel="+ Şube Ekle" />
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14 }}>
        {STORES.map((st, i) => {
          const p = spring({ frame: frame - 8 - i * 14, fps, config: { damping: 14 } });
          return (
            <div key={st.name} style={{
              background: C.card, borderRadius: 14, padding: "18px 20px",
              boxShadow: "0 2px 16px rgba(79,158,248,.1)",
              opacity: p, transform: `translateY(${interpolate(p, [0, 1], [20, 0])}px)`,
            }}>
              <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 14 }}>
                <div style={{ width: 38, height: 38, borderRadius: 10, background: `${st.color}18`, display: "flex", alignItems: "center", justifyContent: "center" }}>
                  <IStore s={20} c={st.color} />
                </div>
                <div>
                  <div style={{ fontWeight: 700, fontSize: 14, color: C.text }}>{st.name}</div>
                  <div style={{ fontSize: 11, color: C.muted }}>{st.addr}</div>
                </div>
              </div>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end" }}>
                <div>
                  <div style={{ fontSize: 26, fontWeight: 800, color: st.color }}>{st.qr}</div>
                  <div style={{ fontSize: 11, color: C.muted }}>QR Okutma</div>
                </div>
                <Badge label="Yönet →" color={st.color} />
              </div>
            </div>
          );
        })}
      </div>
    </AbsoluteFill>
  );
}

// ── SAHNE 3: Şube detayı + QR ─────────────────────────────────────────────
function Scene3() {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  return (
    <AbsoluteFill style={{ padding: "28px 36px", background: C.bg }}>
      <FadeUp delay={0} style={{ marginBottom: 16 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <div style={{ width: 40, height: 40, borderRadius: 10, background: `${C.primary}18`, display: "flex", alignItems: "center", justifyContent: "center" }}>
            <IStore s={22} c={C.primary} />
          </div>
          <div>
            <div style={{ fontWeight: 800, fontSize: 16, color: C.text }}>Merkez Şube</div>
            <div style={{ fontSize: 12, color: C.muted }}>Beşiktaş, İstanbul</div>
          </div>
          <Badge label="Aktif" color={C.success} />
        </div>
      </FadeUp>

      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 10, marginBottom: 14 }}>
        {[["234", "QR Okutma", C.primary], ["12", "Ürün", C.success], ["1", "Menü", C.warning]].map(([v, l, c], i) => {
          const p = spring({ frame: frame - 6 - i * 8, fps, config: { damping: 14 } });
          return (
            <div key={l} style={{
              background: C.card, borderRadius: 12, padding: "14px 16px",
              boxShadow: "0 2px 12px rgba(79,158,248,.08)",
              opacity: p, transform: `translateY(${interpolate(p, [0, 1], [16, 0])}px)`,
            }}>
              <div style={{ fontSize: 22, fontWeight: 800, color: c as string }}>{v}</div>
              <div style={{ fontSize: 11, color: C.muted }}>{l}</div>
            </div>
          );
        })}
      </div>

      <Card delay={20}>
        <div style={{ display: "flex", gap: 18, alignItems: "center" }}>
          <div style={{ background: "#f8faff", borderRadius: 12, padding: 12, border: `1.5px solid ${C.border}` }}>
            <IQr s={72} c={C.text} />
          </div>
          <div style={{ flex: 1 }}>
            <div style={{ fontWeight: 800, fontSize: 14, color: C.text, marginBottom: 6 }}>QR Kodunuz Hazır</div>
            <div style={{ fontSize: 12, color: C.muted, marginBottom: 12 }}>
              tummenu.com/guzel-yemekler/merkez-sube
            </div>
            <div style={{ display: "flex", gap: 8 }}>
              <Badge label="Kopyala" color={C.primary} />
              <Badge label="İndir" color={C.muted.replace("a3", "6b")} />
              <Badge label="Paylaş" color={C.success} />
            </div>
          </div>
        </div>
      </Card>
    </AbsoluteFill>
  );
}

// ── SAHNE 4: Bar chart istatistik ─────────────────────────────────────────
const WEEKS = ["Pzt", "Sal", "Çar", "Per", "Cum"];
const DATA: [number, number][] = [[45, 18], [78, 32], [62, 24], [110, 47], [134, 56]];
const MAX = 134;

function Scene4() {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  return (
    <AbsoluteFill style={{ padding: "28px 36px", background: C.bg }}>
      <FadeUp delay={0} style={{ marginBottom: 16 }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <div style={{ fontWeight: 800, fontSize: 16, color: C.text }}>QR Okutma İstatistikleri</div>
            <div style={{ fontSize: 12, color: C.muted }}>Şube bazlı karşılaştırma</div>
          </div>
          <div style={{ display: "flex", gap: 4 }}>
            {["Haftalık", "Aylık"].map((t, i) => (
              <div key={t} style={{
                background: i === 0 ? C.primary : "transparent",
                color: i === 0 ? "#fff" : C.muted,
                borderRadius: 99, padding: "4px 10px", fontSize: 11, fontWeight: 600,
                border: `1px solid ${i === 0 ? C.primary : C.border}`,
              }}>{t}</div>
            ))}
          </div>
        </div>
      </FadeUp>

      <div style={{ background: C.card, borderRadius: 14, padding: "20px 20px 12px", boxShadow: "0 2px 16px rgba(79,158,248,.1)" }}>
        <div style={{ display: "flex", alignItems: "flex-end", gap: 14, height: 160, marginBottom: 8 }}>
          {DATA.map(([m, k], i) => {
            const p = spring({ frame: frame - 10 - i * 8, fps, config: { damping: 12 } });
            const mH = (m / MAX) * 140 * p;
            const kH = (k / MAX) * 140 * p;
            return (
              <div key={i} style={{ flex: 1, display: "flex", gap: 3, alignItems: "flex-end" }}>
                <div style={{ flex: 1, height: mH, background: `${C.primary}cc`, borderRadius: "4px 4px 0 0" }} />
                <div style={{ flex: 1, height: kH, background: `${C.success}cc`, borderRadius: "4px 4px 0 0" }} />
              </div>
            );
          })}
        </div>
        <div style={{ display: "flex", gap: 0 }}>
          {WEEKS.map((w, i) => (
            <div key={w} style={{ flex: 1, textAlign: "center", fontSize: 10, color: C.muted }}>{w}</div>
          ))}
        </div>
        <div style={{ display: "flex", gap: 16, marginTop: 12, paddingTop: 12, borderTop: `1px solid ${C.border}` }}>
          {[["Merkez Şube", C.primary], ["Kadıköy Şube", C.success]].map(([l, c]) => (
            <div key={l} style={{ display: "flex", alignItems: "center", gap: 6, fontSize: 12, color: C.muted }}>
              <div style={{ width: 10, height: 10, borderRadius: 2, background: c as string }} />
              {l}
            </div>
          ))}
        </div>
      </div>
    </AbsoluteFill>
  );
}

// ── Browser title bar ─────────────────────────────────────────────────────
function TitleBar({ url }: { url: string }) {
  return (
    <div style={{ background: "#dde4ef", padding: "10px 18px", display: "flex", alignItems: "center", gap: 8, flexShrink: 0, borderBottom: "1px solid #ccd4e8" }}>
      <div style={{ width: 13, height: 13, borderRadius: "50%", background: "#ff5f57" }} />
      <div style={{ width: 13, height: 13, borderRadius: "50%", background: "#ffbd2e" }} />
      <div style={{ width: 13, height: 13, borderRadius: "50%", background: "#28c840" }} />
      <div style={{ flex: 1, background: "#f0f4fb", borderRadius: 7, padding: "4px 14px", fontSize: 12, color: C.muted, marginLeft: 10, display: "flex", alignItems: "center", gap: 6 }}>
        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke={C.muted} strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
        {url}
      </div>
    </div>
  );
}

// ── Admin sidebar stub ────────────────────────────────────────────────────
function Sidebar() {
  const items = [
    { label: "Panel", active: false, Icon: () => <IMenu s={14} c={C.muted} /> },
    { label: "Şirketler", active: true, Icon: () => <IBuilding s={14} c={C.primary} /> },
    { label: "Şubeler", active: false, Icon: () => <IStore s={14} c={C.muted} /> },
    { label: "İstatistik", active: false, Icon: () => <IChart s={14} c={C.muted} /> },
  ];
  return (
    <div style={{ width: 130, background: "#1e2b4a", display: "flex", flexDirection: "column", padding: "16px 0", gap: 2, flexShrink: 0 }}>
      <div style={{ padding: "0 14px 16px", display: "flex", alignItems: "center", gap: 8, borderBottom: "1px solid rgba(255,255,255,.08)", marginBottom: 6 }}>
        <div style={{ width: 28, height: 28, borderRadius: 8, background: C.primary, display: "flex", alignItems: "center", justifyContent: "center" }}>
          <IMenu s={14} c="#fff" />
        </div>
        <span style={{ fontSize: 12, fontWeight: 800, color: "#fff" }}>TümMenu</span>
      </div>
      {items.map(({ label, active, Icon }) => (
        <div key={label} style={{
          padding: "7px 14px", display: "flex", alignItems: "center", gap: 8,
          background: active ? `${C.primary}22` : "transparent",
          borderLeft: active ? `3px solid ${C.primary}` : "3px solid transparent",
        }}>
          <Icon />
          <span style={{ fontSize: 11, color: active ? C.primary : "rgba(255,255,255,.45)", fontWeight: active ? 700 : 400 }}>{label}</span>
        </div>
      ))}
    </div>
  );
}

// ── ANA KOMPOZİSYON ──────────────────────────────────────────────────────
export const CompanyStoreVideo: React.FC = () => {
  return (
    <AbsoluteFill style={{ background: "#e8edf7", display: "flex", flexDirection: "column" }}>
      <TitleBar url="tummenu.com/admin/sirketler" />
      <div style={{ flex: 1, display: "flex", overflow: "hidden" }}>
        <Sidebar />
        <div style={{ flex: 1, position: "relative", overflow: "hidden" }}>
          <Sequence from={0} durationInFrames={110}><Scene1 /></Sequence>
          <Sequence from={110} durationInFrames={130}><Scene2 /></Sequence>
          <Sequence from={240} durationInFrames={150}><Scene3 /></Sequence>
          <Sequence from={390} durationInFrames={150}><Scene4 /></Sequence>
        </div>
      </div>
    </AbsoluteFill>
  );
};
