# HR Design System V3 — Ultra Futuristic Enterprise

> **Status:** Approved visual identity baseline; implementation specification for Claude Artifacts.  
> **Product name:** **HR** (not “Nexus HR”).  
> **Languages:** Arabic (RTL, default) and English (LTR).  
> **Themes:** Light and Dark, equally supported.  
> **Scope:** Visual identity, UI components, UX patterns, interaction, motion, accessibility, and handoff. **Not** a source of new HR business requirements.

## 1. Source of truth and precedence

1. **Product requirements (`HR-Requirements.md`)** define which business features and workflows exist.
2. **API contract** defines endpoint paths, methods, fields, enums, validation and error shapes; never invent contract details.
3. **This design system** defines appearance, component behavior, interaction, responsive design and motion.
4. **Official supplied light/dark brand reference images and original logo asset** define the visual identity. If a reference image and a decorative suggestion here disagree, preserve the official brand reference and request clarification.

Do not introduce a business module merely because a component is described here. Components and example screens are reusable patterns, not new requirements.

## 2. Brand identity

### 2.1 Name and message

- Official product name in navigation, browser titles and documentation: **HR**.
- Expanded descriptor where useful: **Human Resources Management System**.
- Never display “Nexus HR” as the product name. Do not automatically rename backend identifiers, API routes or database entities.
- Brand character: connected people, institutional clarity, intelligent management, precision, trust, and human-centered efficiency.

### 2.2 Logo

The official logo is an interlocking geometric mark suggesting **H/R** and connected circular relationships. Preserve its actual proportions and geometry from the supplied artwork.

- **Light:** charcoal/dark slate mark with emerald-teal accents on light surfaces.
- **Dark:** white/light mark with luminous emerald-teal accents on deep slate surfaces.
- Use the original SVG or transparent PNG if available. **Do not trace, approximate or replace it with a generic N/HR icon** without approval.
- Maintain clear space of at least 25% of the logo-mark height on every side; do not distort, rotate, recolor arbitrarily, stretch, mask or add permanent excessive glow.
- A subtle one-time opacity/translate entrance is permitted; keep the logo legible and still afterward.
- In small navigation, use the official symbol-only asset if available; otherwise use the full approved asset at a readable size.

### 2.3 Official reference artwork

The supplied **dark and light identity reference images** are required inputs for visual review. They include UI samples, iconography and brand treatments. The exact original image files should accompany this Markdown when handed to Claude. Screenshots are references, not automatically pixel-perfect specifications for every screen.

## 3. Color system

### 3.1 Official theme tokens

| Semantic token | Light | Dark |
|---|---|---|
| `--hr-bg` | `#F8FAFC` | `#0F172A` |
| `--hr-surface` | `#FFFFFF` | `#1E293B` |
| `--hr-text-primary` | `#0F172A` | `#F8FAFC` |
| `--hr-text-secondary` | `#475569` | `#94A3B8` |
| `--hr-accent` | `#00A887` | `#00C49F` |
| `--hr-border` | `#E2E8F0` | `#334155` |

### 3.2 Semantic status colors

| Meaning | Value | Usage |
|---|---|---|
| Success | `#10B981` | Active/completed/approved |
| Warning | `#F59E0B` | Pending/attention |
| Danger | `#EF4444` | Rejected/error/destructive |
| Info | `#3B82F6` | Neutral information |

Never convey status by color alone: include a label and, where helpful, an icon. Check text contrast on all filled buttons and badges; **do not assume white text on the light-theme teal is accessible**. Prefer dark text on the official light teal where needed to satisfy contrast.

### 3.3 Decorative colors — not replacements

- Dark emerald glow: `rgba(0,196,159,.20)`
- Soft teal light: `rgba(20,184,166,.12)`
- Cyan edge highlight: `rgba(34,211,238,.16)`
- Dark translucent overlay: `rgba(15,23,42,.72)`

Use cyan sparingly as secondary lighting, not as the main brand color. **Do not reintroduce purple-dominant UI.** Avoid fluorescent, gaming-like saturation.

### 3.4 Suggested CSS tokens

```css
:root, [data-theme="light"] {
  --hr-bg: #F8FAFC;
  --hr-surface: #FFFFFF;
  --hr-text-primary: #0F172A;
  --hr-text-secondary: #475569;
  --hr-accent: #00A887;
  --hr-border: #E2E8F0;
  --hr-shadow-card: 0 4px 16px rgba(15,23,42,.05);
  --hr-glow: rgba(0,168,135,.12);
}
[data-theme="dark"] {
  --hr-bg: #0F172A;
  --hr-surface: #1E293B;
  --hr-text-primary: #F8FAFC;
  --hr-text-secondary: #94A3B8;
  --hr-accent: #00C49F;
  --hr-border: #334155;
  --hr-shadow-card: 0 4px 20px rgba(0,0,0,.35);
  --hr-glow: rgba(0,196,159,.20);
}
:root {
  --hr-success: #10B981;
  --hr-warning: #F59E0B;
  --hr-danger: #EF4444;
  --hr-info: #3B82F6;
  --hr-radius-card: 16px;
  --hr-radius-control: 10px;
  --hr-radius-badge: 20px;
  --hr-focus-ring: 0 0 0 3px var(--hr-glow);
}
```

## 4. Typography and content

| Style | Size / line height | Weight |
|---|---|---|
| H1 / page title | 28px / 36px | 700 |
| H2 / section or card title | 18px / 24px | 600 |
| Body | 14px / 20px | 400 |
| Caption | 12px / 16px | 500 |

- Arabic: **Cairo** or **Tajawal** (choose one consistently per deployment).
- English: **Inter**.
- Use `font-variant-numeric: tabular-nums` for metrics, money, times and tabular numbers.
- Support text wrapping and zoom; never force fixed-height cards that clip Arabic text.
- Localize labels, help text, empty states, validation, notifications, date/time formats, number and currency display.
- Preserve identifiers and codes in their intended direction using appropriate bidirectional isolation.
- Avoid decorative gradients on paragraphs and dense table content.

## 5. Spacing, shape and elevation

- Base spacing scale: **4px increments**, with an **8px rhythm** for major layout.
- Card radius: **16px**; input/button radius: **10px**; status pill radius: **20px**.
- Baseline shadows: light `0 4px 16px rgba(15,23,42,.05)`; dark `0 4px 20px rgba(0,0,0,.35)`.
- Use elevated shadows only for selected/hovered/overlay surfaces.
- Borders remain visible even where glass effects are used.
- Glass panels are optional decorative enhancements: never sacrifice legibility, performance or contrast.

## 6. Layout and responsive rules

- **Desktop ≥ 1280px:** full navigation rail/sidebar, generous content width, multi-column bento and analytics.
- **Tablet 768–1279px:** collapsible sidebar, reduced card columns, responsive chart legends.
- **Mobile < 768px:** navigation drawer, stacked content, touch-safe actions, cards instead of unusably narrow tables when appropriate.
- Target minimum pointer/touch hit areas of about **44×44px** for important controls.
- Never rely on hover to reveal a critical action on touch devices.
- Use logical CSS properties (`margin-inline`, `inset-inline-start`, etc.) and direction-aware icon orientation.
- Do not mirror data visualizations or logos blindly; only mirror direction-sensitive navigation/flow where meaningful.
- No horizontal page overflow; tables may scroll within bounded regions if needed.

## 7. Application shell

### 7.1 Sidebar

- Official HR mark, app label, accessible navigation, active state, optional collapse and user area.
- Active indicator slides smoothly; hover state is distinguishable from selected state.
- Collapsed icons have accessible names/tooltips; expanded state retains legible labels.
- RTL places navigation appropriately and adjusts motion origin.
- Preserve navigation items exactly as specified by requirements and permissions.

### 7.2 Top bar

- Page context/breadcrumbs, existing search, notifications, language/theme toggles and account controls when present in requirements.
- Avoid adding functional search, notifications or shortcuts unless supported by requirements; styling an existing control is allowed.
- Keep header useful on small screens; collapse secondary controls into accessible menus.

### 7.3 Dashboard

- Premium asymmetric bento layout with a clear hero, core KPIs, primary charts and supporting panels **only for data and widgets defined in requirements**.
- Featured KPI cards may use count-up, sparklines, hover elevation and restrained spotlight.
- Dense data tables should remain quiet and legible, with no continuous decorative motion.

## 8. Component specifications

Every component needs: default, hover (where relevant), focus-visible, active/selected, disabled, loading, error and success states as applicable.

| Component | Design and behavior |
|---|---|
| Primary button | Brand teal; accessible foreground; 10px radius; 150–220ms press/hover; loading/disabled |
| Secondary button | Transparent or surface fill, border token, clear focus ring |
| Destructive button | Danger semantic color and confirmation for destructive actions |
| Text input / textarea | Clear label, optional help text, error text, focus ring, no placeholder-only labels |
| Select / combobox | Keyboard operable, localized search, correct overlay direction |
| Date / time picker | Locale display, explicit timezone policy from requirements, keyboard access |
| Checkbox / radio / switch | Visible state, large hit target, labels and validation |
| KPI card | Label, value, unit, trend/context, optional icon/sparkline; reduced-motion alternative |
| Data table | Semantic headers, sorting/filtering only if supported, pagination, sticky header when useful |
| Status badge | Semantic text + optional icon; no color-only meaning |
| Tabs | Keyboard navigable; sliding indicator without hiding content |
| Modal / drawer | Focus trap, escape/close rules, focus restoration, accessible heading |
| Toast | Non-blocking feedback, screen-reader announcement, appropriate persistence |
| Tooltip | Supplementary only; never sole location for required instructions |
| Chart | Accessible summary/table alternative, tooltip, units, legend, color-safe distinction |
| Empty state | Explain what is missing and offer only supported next actions |
| Error state | Specific explanation, retry if possible, never fabricate backend outcome |
| Skeleton | Match final geometry, shimmer only while loading |
| Avatar | Approved image or initials fallback; accessible name |
| Timeline | Timestamp, actor/status where supported, clear sequence |
| File upload UI | File constraints from requirements/API; progress only when actual/simulated transfer exists |

### 8.1 Forms and validation

- Derive fields, required flags, enums, length/range limits and dependencies from **API Contract + requirements**.
- Show inline errors near fields and summary when appropriate.
- Prevent duplicate submissions, preserve entered values after recoverable failures.
- Distinguish client validation from server errors.
- Use confirmation for irreversible actions when required.

### 8.2 Tables and filters

- Preserve exact backend semantics for pagination, sorting and filtering.
- Clearly distinguish loading, no data, no search results and request failure.
- Show relevant row actions only for authorized roles; backend must enforce permissions in real deployment.

### 8.3 Data visualizations

- Display real or explicitly labeled mock values; never invent operational analytics.
- Use consistent time periods, axis labels, currency units and localized formatting.
- Prefer line/bar/donut only when appropriate to the actual metric.

## 9. Signature HR visual: Organizational Network

**Replace the old rotating radar circle** in the welcome hero with a branded **HR Organizational Network** visualization, inspired by the connected H/R mark.

- One central organization/management node.
- Three to four department nodes, plus smaller employee nodes, **as decorative abstractions**, not factual headcounts unless data supports them.
- Curved SVG connections; subtle emerald pulses travel along selected paths.
- Intro sequence: central node appears → departments reveal → paths draw → employee nodes reveal.
- Continuous state: faint 3–6s connection pulses and small node drift; no full-scene repeated reset.
- Initial reveal: about **1.8–2.6s**.
- Keep behind/adjacent to welcome text; never overlap labels or reduce contrast.
- No radar rings, large spinning discs, excessive particles or unrelated sci-fi symbols.
- For reduced-motion users, show a beautiful static network.
- On mobile, simplify node count or show a static compact variant.

## 10. Motion tokens and rules

| Pattern | Target duration | Behavior |
|---|---:|---|
| Page transition | 400–550ms | Opacity + short translate |
| Card entrance | 450–650ms | Opacity + translate, stagger 70–100ms |
| Button hover / press | 150–220ms | Small transform and color change |
| Modal entrance | 350–450ms | Backdrop fade + spring-like scale |
| KPI count-up | 1100–1600ms | From 0 to actual value, locale formatting |
| Chart reveal | 900–1400ms | SVG line draw / bar growth |
| Organization network intro | 1800–2600ms | Staged nodes and paths |
| Network connection pulse | 3–6s | Subtle continuous path light |
| Ambient gradient | 15–25s | Low-opacity slow movement |

- Animate **transform and opacity** whenever possible; avoid animating expensive layout/blur properties continuously.
- Keep animation purposeful: explain hierarchy, feedback, navigation or data change.
- Do not animate large portions of dense tables repeatedly.
- Honor `prefers-reduced-motion: reduce` by removing nonessential movement and preserving state clarity.
- Avoid animation replay on every trivial state change.
- Pause or simplify continuous decoration in low-power contexts where practical.

## 11. Visual and motion enhancement catalog (55 items)

These are **design capabilities**, not requirements to place every effect on every screen. Implement only where the component exists and the effect helps usability.

### A. Visual design (1–15)

1. Emerald aurora ambient background.
2. Layered brand-aware glass surfaces.
3. Asymmetric bento dashboard composition.
4. Pointer-following spotlight on featured cards.
5. Subtle pointer-responsive 3D tilt.
6. Refined emerald gradient borders.
7. Depth-based shadow hierarchy.
8. Floating/translucent top navigation.
9. Distinct surface elevation levels.
10. Premium branded icon containers.
11. Semantic, refined status pills.
12. Selective gradient hero typography.
13. Theme-consistent scrollbars.
14. Subtle nonintrusive background texture.
15. Responsive bento rearrangement.

### B. Motion (16–30)

16. Coordinated dashboard entrance.
17. Staggered KPI reveal.
18. Animated KPI count-up.
19. SVG chart line drawing.
20. Animated bar growth.
21. Donut/progress reveal.
22. Spring-like sidebar collapse.
23. Sliding active navigation indicator.
24. Page enter/exit transitions.
25. Shared layout transition for existing tabs/panels.
26. Spring-like modal entrance.
27. Sliding drawer transition.
28. Subtle staggered table rows.
29. Sliding tab indicator.
30. Notification entry/exit and stacking.

### C. Micro-interactions (31–45)

31. Subtle magnetic-style primary button feedback.
32. Button press scale.
33. Card hover elevation.
34. Emerald edge glow on featured cards.
35. Search focus expansion (if search exists).
36. Dropdown reveal.
37. Tooltip fade/scale.
38. Input focus glow.
39. Smooth existing toggle motion.
40. Loading skeleton shimmer.
41. Progress bar animation.
42. Avatar hover ring.
43. Status change transition.
44. Stable RTL/LTR language switch.
45. Smooth theme color transition.

### D. Brand-specific and quality (46–55)

46. Official logo entrance micro-animation.
47. HR Organizational Network connection pulses.
48. Contextual emerald hero lighting.
49. Theme-aware chart palette transitions.
50. Reduced-complexity mobile motion.
51. Branded empty-state treatment.
52. Breadcrumb transitions.
53. Contextual hover intensity by component density.
54. Coordinated loading indicators.
55. Shared motion tokens and consistency audit.

## 12. Light and dark parity

**Dark:** Deep Slate background, Slate 800 cards, Emerald Teal accents, limited cyan highlights, legible chart labels, restrained glow.  
**Light:** Cool gray background, white cards, Emerald accents, delicate mint highlights, subtle shadows, no muddy translucent overlays.

Both themes must be intentionally designed, not simple color inversions. All states, charts, modal backdrops, hover effects and focus indicators require theme-specific QA.

## 13. Accessibility baseline

- Target **WCAG 2.2 AA** where applicable.
- Body text contrast generally at least 4.5:1; large text at least 3:1; relevant non-text indicators at least 3:1.
- Visible keyboard focus, logical tab order, semantic landmarks and headings.
- Accessible modal focus management and escape behavior.
- Support screen-reader announcements for async results and form errors.
- Do not rely on motion, color or hover alone to communicate meaning.
- Support 200% zoom and reduced motion without losing core workflows.
- Localize accessibility labels in Arabic and English.

## 14. API contract and requirements integration

For each implemented screen, maintain traceability:

| Field | Description |
|---|---|
| Screen ID | Stable identifier |
| Requirement IDs | Source requirements served |
| Roles | Authorized user types from requirements |
| API operations | Exact method/path/operation ID from contract |
| Data fields | Exact request/response names, types and nullability |
| States | Loading, empty, error, success, forbidden, validation |
| Design components | Reusable components used |
| Motion | Purposeful effects applied |
| Tests | Acceptance and interaction evidence |

**Never invent endpoints, roles, fields, status values, validation or business logic.** If the contract and requirements disagree, document the conflict and request resolution. If a live API is unavailable inside Claude Artifacts, use a clearly labeled mock adapter with data structures matching the actual contract; do not claim production connectivity.

## 15. Claude Artifacts implementation constraints

1. Inspect the **existing** HR Artifact and identify its supported framework and dependencies.
2. Preserve current business flows and user data; do not create a separate unrelated project.
3. Implement design tokens and reusable components first.
4. Prefer supported React/CSS/SVG techniques; use native CSS/Web Animations API when a motion library is unavailable.
5. Keep animations actually mounted in rendered components, not merely defined in unused code.
6. Verify preview rendering before claiming success; report untested interactions explicitly.
7. Update in small, reviewable phases and preserve a recoverable version after each phase.
8. Keep all visuals bilingual and theme-aware.
9. Do not use external services or unsupported libraries as prerequisites for the Artifact preview.
10. Do not change API endpoints or internal identifiers just to rename visible branding.

## 16. Recommended delivery phases

1. **Brand foundation:** Official logo, tokens, fonts, light/dark and RTL/LTR.
2. **App shell:** Sidebar, top bar, navigation, global components.
3. **Signature dashboard:** Bento, KPIs, charts, HR Organizational Network.
4. **Component library:** Forms, tables, badges, overlays, states.
5. **Requirements-based screens:** Implement one module at a time using approved requirements/API mapping.
6. **QA and handoff:** Traceability, visual parity, motion, accessibility and functional coverage.

## 17. Acceptance checklist

- [ ] Product is called **HR**, not Nexus HR, in all visible branding.
- [ ] Official logo asset is used without unapproved redrawing.
- [ ] Official Light and Dark colors match specified tokens.
- [ ] Arabic RTL and English LTR both work.
- [ ] Typography, radii, spacing, shadows and focus are consistent.
- [ ] Sidebar, header, dashboard and shared components follow this system.
- [ ] HR Organizational Network replaces the radar and animates in preview.
- [ ] Motion features claimed as complete are visibly implemented.
- [ ] Reduced-motion mode retains full usability.
- [ ] Loading, empty, error, success and permission states are designed where applicable.
- [ ] Mobile, tablet and desktop layouts are usable.
- [ ] All implemented business screens map to approved requirements.
- [ ] All API interactions map to exact contract operations or clearly labeled mocks.
- [ ] No unsupported business features or invented API endpoints were introduced.
- [ ] Actual tests are distinguished from untested assumptions.

## 18. Reusable Claude instruction

> Read `HR-Design-System.md`, the official HR logo and the two supplied brand reference images. Treat them as the visual source of truth. Read `HR-Requirements.md` and the API Contract for business scope and data contracts. Work on the existing HR Claude Artifact only. Implement brand-consistent, bilingual Light/Dark UI and purposeful Ultra Futuristic motion. Preserve all existing functionality; do not invent business features, endpoints or data. Implement incrementally, show a working preview, and report verified versus untested behavior.

---

**Document version:** 3.0  
**Brand:** HR  
**Intended use:** Claude Artifacts UI/UX implementation and future developer handoff.  
**Reference dependency:** Supply the original official HR logo and both user-provided visual identity images alongside this file.
