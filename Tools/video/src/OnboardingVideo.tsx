import {
  AbsoluteFill,
  interpolate,
  spring,
  useCurrentFrame,
  useVideoConfig,
  Sequence,
} from "remotion";

// ── Renk paleti (TumMenu) ──────────────────────────────────────────────────
const C = {
  primary: "#4f9ef8",
  success: "#38d9a9",
  warning: "#f7c948",
  info: "#4fc3f7",
  danger: "#f06595",
  bg: "#f0f4fb",
  card: "#ffffff",
  text: "#2b3674",
  muted: "#a3aed0",
  border: "#e8edf7",
};

// ── SVG İkonları ──────────────────────────────────────────────────────────
function IconBuilding({ size = 40, color = C.primary }: { size?: number; color?: string }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
      <rect x="3" y="3" width="18" height="18" rx="2" />
      <path d="M3 9h18" />
      <path d="M9 21V9" />
      <rect x="12" y="13" width="3" height="3" />
      <rect x="12" y="17" width="3" height="3" />
    </svg>
  );
}

function IconStore({ size = 40, color = C.success }: { size?: number; color?: string }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
      <path d="M3 9l1-6h16l1 6" />
      <path d="M3 9a3 3 0 0 0 6 0 3 3 0 0 0 6 0 3 3 0 0 0 6 0" />
      <path d="M5 21V9.5" />
      <path d="M19 21V9.5" />
      <rect x="8" y="14" width="8" height="7" rx="1" />
    </svg>
  );
}

function IconUtensils({ size = 40, color = C.info }: { size?: number; color?: string }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
      <path d="M3 2v7c0 2.2 1.8 4 4 4s4-1.8 4-4V2" />
      <path d="M7 2v20" />
      <path d="M21 15V2a5 5 0 0 0-5 5v6h3v7" />
    </svg>
  );
}

function IconTag({ size = 36, color = C.warning }: { size?: number; color?: string }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
      <path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z" />
      <line x1="7" y1="7" x2="7.01" y2="7" strokeWidth="2.5" />
    </svg>
  );
}


function IconMenu({ size = 18, color = "#fff" }: { size?: number; color?: string }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2.2" strokeLinecap="round">
      <line x1="3" y1="6" x2="21" y2="6" />
      <line x1="3" y1="12" x2="21" y2="12" />
      <line x1="3" y1="18" x2="21" y2="18" />
    </svg>
  );
}

function IconGrip({ size = 16, color = C.muted }: { size?: number; color?: string }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill={color}>
      <circle cx="9" cy="6" r="1.5" /><circle cx="15" cy="6" r="1.5" />
      <circle cx="9" cy="12" r="1.5" /><circle cx="15" cy="12" r="1.5" />
      <circle cx="9" cy="18" r="1.5" /><circle cx="15" cy="18" r="1.5" />
    </svg>
  );
}

function IconCheck({ size = 14, color = "#fff" }: { size?: number; color?: string }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
      <polyline points="20 6 9 17 4 12" />
    </svg>
  );
}

function IconLock({ size = 14, color = C.muted }: { size?: number; color?: string }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <rect x="3" y="11" width="18" height="11" rx="2" />
      <path d="M7 11V7a5 5 0 0 1 10 0v4" />
    </svg>
  );
}

function IconQr({ size = 32, color = C.primary }: { size?: number; color?: string }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
      <rect x="3" y="3" width="7" height="7" rx="1" />
      <rect x="14" y="3" width="7" height="7" rx="1" />
      <rect x="3" y="14" width="7" height="7" rx="1" />
      <rect x="5" y="5" width="3" height="3" fill={color} stroke="none" />
      <rect x="16" y="5" width="3" height="3" fill={color} stroke="none" />
      <rect x="5" y="16" width="3" height="3" fill={color} stroke="none" />
      <line x1="14" y1="14" x2="14" y2="14" strokeWidth="3" />
      <line x1="17" y1="14" x2="21" y2="14" />
      <line x1="14" y1="17" x2="14" y2="21" />
      <line x1="17" y1="17" x2="21" y2="17" />
      <line x1="17" y1="21" x2="21" y2="21" />
    </svg>
  );
}

// ── Yardımcı: yazı yazma efekti ───────────────────────────────────────────
function TypedText({
  text,
  startFrame,
  charsPerFrame = 1.5,
  style,
}: {
  text: string;
  startFrame: number;
  charsPerFrame?: number;
  style?: React.CSSProperties;
}) {
  const frame = useCurrentFrame();
  const elapsed = Math.max(0, frame - startFrame);
  const chars = Math.min(text.length, Math.floor(elapsed * charsPerFrame));
  return (
    <span style={style}>
      {text.slice(0, chars)}
      {chars < text.length && (
        <span
          style={{
            borderRight: `2px solid ${C.primary}`,
            marginLeft: 1,
            opacity: Math.floor(elapsed / 8) % 2 === 0 ? 1 : 0,
          }}
        />
      )}
    </span>
  );
}

// ── Yardımcı: fade+yukarı girme ───────────────────────────────────────────
function FadeUp({
  children,
  delay = 0,
  style,
}: {
  children: React.ReactNode;
  delay?: number;
  style?: React.CSSProperties;
}) {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  const progress = spring({ frame: frame - delay, fps, config: { damping: 16, stiffness: 80 } });
  return (
    <div
      style={{
        opacity: interpolate(progress, [0, 1], [0, 1]),
        transform: `translateY(${interpolate(progress, [0, 1], [24, 0])}px)`,
        ...style,
      }}
    >
      {children}
    </div>
  );
}

// ── Ortak: progress bar ───────────────────────────────────────────────────
function ProgressBar({ step }: { step: number }) {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  const pct = spring({ frame, fps, config: { damping: 20 } }) * (step / 3) * 100;
  return (
    <div style={{ height: 5, background: C.border, borderRadius: 99 }}>
      <div
        style={{
          height: "100%",
          width: `${pct}%`,
          background: `linear-gradient(90deg, ${C.primary}, ${C.success})`,
          borderRadius: 99,
        }}
      />
    </div>
  );
}

// ── Sahte form satırı ─────────────────────────────────────────────────────
function FakeInput({
  label,
  value,
  startFrame,
  color = C.primary,
}: {
  label: string;
  value: string;
  startFrame: number;
  color?: string;
}) {
  const frame = useCurrentFrame();
  const visible = frame >= startFrame - 5;
  return (
    <div style={{ marginBottom: 20, opacity: visible ? 1 : 0 }}>
      <div style={{ fontSize: 13, fontWeight: 600, color: C.muted, marginBottom: 6 }}>
        {label}
      </div>
      <div
        style={{
          border: `2px solid ${frame >= startFrame ? color : C.border}`,
          borderRadius: 10,
          padding: "10px 14px",
          background: "#fff",
          fontSize: 17,
          fontWeight: 600,
          color: C.text,
          minHeight: 44,
        }}
      >
        <TypedText text={value} startFrame={startFrame} />
      </div>
    </div>
  );
}

// ── Sahte buton ───────────────────────────────────────────────────────────
function FakeButton({
  label,
  pressFrame,
  color = C.primary,
}: {
  label: string;
  pressFrame: number;
  color?: string;
}) {
  const frame = useCurrentFrame();
  const pressed = frame >= pressFrame;
  return (
    <div
      style={{
        background: pressed ? color : `${color}cc`,
        color: "#fff",
        borderRadius: 10,
        padding: "13px 20px",
        fontWeight: 700,
        fontSize: 16,
        textAlign: "center",
        transform: pressed ? "scale(0.97)" : "scale(1)",
        boxShadow: pressed ? "none" : `0 4px 18px ${color}55`,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        gap: 8,
      }}
    >
      {label}
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#fff" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
        <line x1="5" y1="12" x2="19" y2="12" />
        <polyline points="12 5 19 12 12 19" />
      </svg>
    </div>
  );
}

// ── İkon Kutusu ───────────────────────────────────────────────────────────
function StepIconBox({ children, bg }: { children: React.ReactNode; bg: string }) {
  return (
    <div
      style={{
        width: 84,
        height: 84,
        borderRadius: 22,
        background: bg,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        margin: "0 auto 12px",
      }}
    >
      {children}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// ADIM 1 – Hoşgeldiniz
// ═══════════════════════════════════════════════════════════════════════════
function Step1() {
  return (
    <AbsoluteFill style={{ padding: "32px 40px" }}>
      <ProgressBar step={0} />
      <FadeUp delay={5} style={{ textAlign: "center", marginTop: 28, marginBottom: 24 }}>
        <StepIconBox bg={`${C.primary}18`}>
          <IconBuilding size={48} color={C.primary} />
        </StepIconBox>
        <h2 style={{ fontSize: 28, fontWeight: 800, color: C.primary, margin: "8px 0 4px" }}>
          Hoşgeldiniz!
        </h2>
        <p style={{ fontSize: 15, color: C.muted, margin: 0 }}>
          Hemen başlayalım. Şirketinizin adı nedir?
        </p>
      </FadeUp>
      <FadeUp delay={12}>
        <FakeInput label="Şirket Adı" value="Güzel Yemekler A.Ş." startFrame={20} />
        <div style={{ fontSize: 13, fontWeight: 600, color: C.muted, marginBottom: 6 }}>
          Şirket Linki
        </div>
        <div
          style={{
            border: `2px solid ${C.border}`,
            borderRadius: 10,
            padding: "10px 14px",
            background: "#f8faff",
            fontSize: 15,
            color: C.muted,
            marginBottom: 24,
            display: "flex",
            alignItems: "center",
            gap: 4,
          }}
        >
          <IconLock size={13} color={C.muted} />
          &nbsp;tummenu.com/&nbsp;
          <TypedText text="guzel-yemekler" startFrame={38} style={{ color: C.primary }} />
        </div>
        <FakeButton label="Şirketi Oluştur ve Devam Et" pressFrame={75} color={C.primary} />
      </FadeUp>
    </AbsoluteFill>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// ADIM 2 – Şube
// ═══════════════════════════════════════════════════════════════════════════
function Step2() {
  return (
    <AbsoluteFill style={{ padding: "32px 40px" }}>
      <ProgressBar step={1} />
      <FadeUp delay={5} style={{ textAlign: "center", marginTop: 28, marginBottom: 24 }}>
        <StepIconBox bg={`${C.success}18`}>
          <IconStore size={48} color={C.success} />
        </StepIconBox>
        <h2 style={{ fontSize: 28, fontWeight: 800, color: C.success, margin: "8px 0 4px" }}>
          Harika Gidiyoruz!
        </h2>
        <p style={{ fontSize: 15, color: C.muted, margin: 0 }}>
          Şimdi ilk şubemizi ekleyelim.
        </p>
      </FadeUp>
      <FadeUp delay={12}>
        <FakeInput label="Şube Adı" value="Merkez Şube" startFrame={20} color={C.success} />
        <FakeInput label="Telefon" value="0532 123 45 67" startFrame={40} color={C.success} />
        <div
          style={{
            border: `2px solid ${C.border}`,
            borderRadius: 10,
            padding: "10px 14px",
            background: "#f8faff",
            fontSize: 15,
            color: C.muted,
            marginBottom: 24,
            display: "flex",
            alignItems: "center",
            gap: 4,
          }}
        >
          <IconLock size={13} color={C.muted} />
          &nbsp;tummenu.com/guzel-yemekler/&nbsp;
          <TypedText text="merkez-sube" startFrame={50} style={{ color: C.success }} />
        </div>
        <FakeButton label="Şubeyi Kaydet ve Devam Et" pressFrame={75} color={C.success} />
      </FadeUp>
    </AbsoluteFill>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// ADIM 3 – Menü
// ═══════════════════════════════════════════════════════════════════════════
function Step3() {
  return (
    <AbsoluteFill style={{ padding: "32px 40px" }}>
      <ProgressBar step={2} />
      <FadeUp delay={5} style={{ textAlign: "center", marginTop: 28, marginBottom: 24 }}>
        <StepIconBox bg={`${C.info}18`}>
          <IconUtensils size={48} color={C.info} />
        </StepIconBox>
        <h2 style={{ fontSize: 28, fontWeight: 800, color: C.info, margin: "8px 0 4px" }}>
          Ana Menünüzü Oluşturun
        </h2>
        <p style={{ fontSize: 15, color: C.muted, margin: 0 }}>
          Bu menüyü ileride tüm şubelerinizde kullanabilirsiniz.
        </p>
      </FadeUp>
      <FadeUp delay={12}>
        <FakeInput label="Menü Adı" value="Ana Menü" startFrame={20} color={C.info} />
        <FakeButton label="Kategori Ekle" pressFrame={60} color={C.info} />
      </FadeUp>
    </AbsoluteFill>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// ADIM 4 – Kategoriler
// ═══════════════════════════════════════════════════════════════════════════
const CATS = ["Başlangıçlar", "Ana Yemekler", "Tatlılar", "İçecekler", "Salatalar"];

function CategoryChip({ title, appearsAt }: { title: string; appearsAt: number }) {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  const p = spring({ frame: frame - appearsAt, fps, config: { damping: 14 } });
  return (
    <div
      style={{
        opacity: p,
        transform: `scale(${interpolate(p, [0, 1], [0.7, 1])})`,
        background: `${C.warning}22`,
        border: `2px solid ${C.warning}`,
        borderRadius: 10,
        padding: "8px 12px",
        fontWeight: 700,
        fontSize: 14,
        color: C.text,
        display: "flex",
        alignItems: "center",
        gap: 6,
      }}
    >
      <IconGrip size={14} color={C.muted} />
      {title}
      <div
        style={{
          marginLeft: "auto",
          background: C.danger,
          borderRadius: 6,
          width: 20,
          height: 20,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
        }}
      >
        <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="#fff" strokeWidth="3" strokeLinecap="round">
          <line x1="18" y1="6" x2="6" y2="18" />
          <line x1="6" y1="6" x2="18" y2="18" />
        </svg>
      </div>
    </div>
  );
}

function Step4() {
  const frame = useCurrentFrame();
  const complete = frame >= 80;
  return (
    <AbsoluteFill style={{ padding: "32px 40px" }}>
      <ProgressBar step={3} />
      <FadeUp delay={5} style={{ textAlign: "center", marginTop: 16, marginBottom: 20 }}>
        <StepIconBox bg={`${C.warning}18`}>
          <IconTag size={46} color={C.warning} />
        </StepIconBox>
        <h2 style={{ fontSize: 26, fontWeight: 800, color: C.warning, margin: "8px 0 4px" }}>
          İlk Kategorilerin!
        </h2>
        <p style={{ fontSize: 14, color: C.muted, margin: 0 }}>
          Kütüphaneden kategorileri seç ve menüne ekle.
        </p>
      </FadeUp>
      <FadeUp delay={12}>
        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 10, marginBottom: 20 }}>
          {CATS.map((cat, i) => (
            <CategoryChip key={cat} title={cat} appearsAt={15 + i * 10} />
          ))}
        </div>
        <div
          style={{
            background: complete ? C.primary : `${C.primary}99`,
            color: "#fff",
            borderRadius: 12,
            padding: "14px 20px",
            fontWeight: 800,
            fontSize: 17,
            textAlign: "center",
            transform: complete ? "scale(0.97)" : "scale(1)",
            boxShadow: complete ? "none" : `0 4px 18px ${C.primary}55`,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            gap: 8,
          }}
        >
          <IconCheck size={16} color="#fff" />
          Kurulumu Tamamla
        </div>
      </FadeUp>
    </AbsoluteFill>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// TAMAMLANDI – Dashboard
// ═══════════════════════════════════════════════════════════════════════════
const STATS = [
  { Icon: IconStore,    iconColor: C.primary, label: "Şube",      val: "1" },
  { Icon: IconUtensils, iconColor: C.success, label: "Menü",      val: "1" },
  { Icon: IconTag,      iconColor: C.warning, label: "Kategori",  val: "5" },
  { Icon: IconQr,       iconColor: C.danger,  label: "QR Tarama", val: "—" },
];

function DashboardFlash() {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  const fadeIn = spring({ frame, fps, config: { damping: 18 } });

  return (
    <AbsoluteFill
      style={{
        background: C.bg,
        padding: "32px 40px",
        opacity: interpolate(fadeIn, [0, 1], [0, 1]),
      }}
    >
      {/* Topbar */}
      <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 28 }}>
        <div
          style={{
            background: C.primary,
            borderRadius: 10,
            width: 36,
            height: 36,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
          }}
        >
          <IconMenu size={18} color="#fff" />
        </div>
        <span style={{ fontWeight: 800, fontSize: 18, color: C.text }}>TümMenu Admin</span>
        <div
          style={{
            marginLeft: "auto",
            background: `${C.success}22`,
            border: `1.5px solid ${C.success}`,
            color: C.success,
            borderRadius: 99,
            padding: "4px 14px",
            fontWeight: 700,
            fontSize: 13,
            display: "flex",
            alignItems: "center",
            gap: 6,
          }}
        >
          <IconCheck size={12} color={C.success} />
          Kurulum Tamamlandı!
        </div>
      </div>

      {/* Stat kartları */}
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14, marginBottom: 20 }}>
        {STATS.map(({ Icon, iconColor, label, val }, i) => {
          const p = spring({ frame: frame - 8 - i * 6, fps, config: { damping: 14 } });
          return (
            <div
              key={label}
              style={{
                background: C.card,
                borderRadius: 14,
                padding: "16px 18px",
                boxShadow: "0 2px 12px rgba(79,158,248,.1)",
                opacity: p,
                transform: `translateY(${interpolate(p, [0, 1], [20, 0])}px)`,
              }}
            >
              <div style={{ marginBottom: 6 }}>
                <Icon size={22} color={iconColor} />
              </div>
              <div style={{ fontSize: 26, fontWeight: 800, color: iconColor }}>{val}</div>
              <div style={{ fontSize: 12, color: C.muted, fontWeight: 600 }}>{label}</div>
            </div>
          );
        })}
      </div>

      {/* QR ipucu */}
      <div
        style={{
          background: `linear-gradient(135deg, ${C.primary}22, ${C.success}22)`,
          border: `1.5px solid ${C.primary}44`,
          borderRadius: 14,
          padding: "14px 18px",
          display: "flex",
          alignItems: "center",
          gap: 14,
          opacity: spring({ frame: frame - 30, fps, config: { damping: 16 } }),
        }}
      >
        <IconQr size={48} color={C.primary} />
        <div>
          <div style={{ fontWeight: 800, fontSize: 15, color: C.text }}>QR kodunuz hazır!</div>
          <div style={{ fontSize: 13, color: C.muted }}>
            tummenu.com/guzel-yemekler adresini müşterilerinizle paylaşın.
          </div>
        </div>
      </div>
    </AbsoluteFill>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// BROWSER TITLE BAR
// ═══════════════════════════════════════════════════════════════════════════
function TitleBar() {
  return (
    <div
      style={{
        background: "#dde4ef",
        padding: "10px 18px",
        display: "flex",
        alignItems: "center",
        gap: 8,
        flexShrink: 0,
        borderBottom: "1px solid #ccd4e8",
      }}
    >
      <div style={{ width: 13, height: 13, borderRadius: "50%", background: "#ff5f57" }} />
      <div style={{ width: 13, height: 13, borderRadius: "50%", background: "#ffbd2e" }} />
      <div style={{ width: 13, height: 13, borderRadius: "50%", background: "#28c840" }} />
      <div
        style={{
          flex: 1,
          background: "#f0f4fb",
          borderRadius: 7,
          padding: "4px 14px",
          fontSize: 13,
          color: C.muted,
          fontFamily: "monospace",
          marginLeft: 10,
          display: "flex",
          alignItems: "center",
          gap: 6,
        }}
      >
        <IconLock size={12} color={C.muted} />
        tummenu.com/admin/onboarding
      </div>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// ANA KOMPOZİSYON
// ═══════════════════════════════════════════════════════════════════════════
export const OnboardingVideo: React.FC = () => {
  return (
    <AbsoluteFill style={{ background: "#e8edf7", display: "flex", flexDirection: "column" }}>
      <TitleBar />
      <div style={{ flex: 1, background: C.bg, position: "relative", overflow: "hidden" }}>
        <div
          style={{
            position: "absolute",
            top: 32,
            bottom: 32,
            left: "50%",
            transform: "translateX(-50%)",
            width: 560,
            background: C.card,
            borderRadius: 20,
            boxShadow: "0 8px 40px rgba(79,158,248,.13)",
            overflow: "hidden",
          }}
        >
          <Sequence from={0} durationInFrames={100}><Step1 /></Sequence>
          <Sequence from={100} durationInFrames={100}><Step2 /></Sequence>
          <Sequence from={200} durationInFrames={100}><Step3 /></Sequence>
          <Sequence from={300} durationInFrames={110}><Step4 /></Sequence>
          <Sequence from={410} durationInFrames={130}><DashboardFlash /></Sequence>
        </div>
      </div>
    </AbsoluteFill>
  );
};
