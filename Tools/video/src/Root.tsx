import "./index.css";
import { Composition } from "remotion";
import { OnboardingVideo } from "./OnboardingVideo";
import { CompanyStoreVideo } from "./CompanyStoreVideo";
import { MenuProductVideo } from "./MenuProductVideo";

export const RemotionRoot: React.FC = () => {
  return (
    <>
      <Composition
        id="OnboardingVideo"
        component={OnboardingVideo}
        durationInFrames={540}
        fps={30}
        width={1280}
        height={720}
      />
      <Composition
        id="CompanyStoreVideo"
        component={CompanyStoreVideo}
        durationInFrames={540}
        fps={30}
        width={1280}
        height={720}
      />
      <Composition
        id="MenuProductVideo"
        component={MenuProductVideo}
        durationInFrames={540}
        fps={30}
        width={1280}
        height={720}
      />
    </>
  );
};
