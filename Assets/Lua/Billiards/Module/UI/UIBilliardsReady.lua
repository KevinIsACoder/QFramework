local UIBilliardsReady = class("UIBilliardsReady", BaseUIBilliardsReady)
UIBilliardsReady:include(UIPrepareViewMixin)

local ModelType = BillardsType.ModelType
local UI = UnityEngine.UI
local BillardsEvent = BillardsEvent.BillardsEventType

function UIBilliardsReady:OnOpened()
    self:_InitUI()
    self:OnPrepareOpened()
    self:RegisterEvent()
    self:RefreshData()
end

function UIBilliardsReady:_InitUI()
    self.menuAtlas = Spark.Assets.LoadAsset("Game/Billiards/Atlas/BillardReady", typeof(Spark.UIAtlas))

    self.view.lblRule.text = I18nManager.GetCurrentValue("Billiards_ReadyRule", true)
    self.view.lblBilliards.text = I18nManager.GetCurrentValue("Billiards_Title", true)

    self.isMember = self.view.isMember.gameObject
    self.MemberGameTypeTxt = self.view.memberText.gameObject:GetComponent(typeof(UI.Text))
    self.roomerModeLabel = self.view.roomerModeLabel.gameObject:GetComponent(typeof(UI.Text))
end

function UIBilliardsReady:RegisterEvent()
    self.eventManager:Add(EventDef.E_Battle_GameType, callback(self.RefreshData, self))

    self.eventManager:Add(EventDef.E_Refresh_GameSeat, callback(self.RefreshSeatInfo, self))

    self.eventManager:Add(EventDef.E_SetMenuBtnSprite, callback(self.SetMenuBtnSprite, self))
end

function UIBilliardsReady:RefreshData()
    self:RefreshGameType(BilliardsBattle.GetModeType())
end

function UIBilliardsReady:RefreshGameType(gameType)
    self._gameType = gameType
    --BilliardsBattle.SetModeType(self._gameType)
    self.MemberGameTypeTxt.text = I18nManager.ApplyRTLfix(self:GetGameTypeString(self._gameType))

    local mode = I18nManager.GetCurrentValue("Mode_1")
    local str = mode .. self:GetGameTypeString(self._gameType) .. ">"
    str = I18nManager.ApplyRTLfix(str)
    self.roomerModeLabel.text = str
end

function UIBilliardsReady:RefreshSeatInfo()
    local isHost = UserInfo.IsHost()

    self.roomerModeLabel:SetActive(isHost)
    self.isMember:SetActive(not isHost)
end

function UIBilliardsReady:GetGameTypeString(gameType)
    local str = ""
    if gameType == ModelType.NORMAL then
        str = I18nManager.GetCurrentValue("Billiards_Normal")
    elseif gameType == ModelType.EASY then
        str = I18nManager.GetCurrentValue("Billiards_Easy")
    end
    return str
end

function UIBilliardsReady:SetMenuBtnSprite(_, evt)
    local image = self.view.menuBtn.gameObject:GetComponent(typeof(UI.Image))
    if evt.isOpen then
        image.sprite = self.menuAtlas:GetSprite("button_Move")
    else
        image.sprite = self.menuAtlas:GetSprite("button_option")
    end
end

function UIBilliardsReady:OpenPublishRule()
    local sub_room_mode = self._gameType

    local showRuleFunc = function (info)
        if info then
            SparkPrefs.SetString(Consts.GameType.Billards .. sub_room_mode, info)
            local entrance = (info.data or {}).entrance or ""
            local title = (info.data or {}).title or ""
            local content = (info.data or {}).content or {}

            local desc = ""
            for key, value in ipairs(content) do
                desc = desc .. value .. "\n"
            end

            local ruleTxt = self.view.publishRuleTitle.gameObject:GetComponent(typeof(UI.Text))
            ruleTxt.fontSize = #title >= 18 and 30 or 40
            ruleTxt.text = I18nManager.ApplyRTLfix(title)

            local descText = self.view.publishRuleDesc.gameObject:GetComponent(typeof(UI.Text))
            descText.text = I18nManager.ApplyRTLfix(desc)

            if I18nManager.IsRTL() then
                descText.alignment = UE.TextAnchor.UpperRight
            else
                descText.alignment = UE.TextAnchor.UpperLeft
            end
            
            self.PublishRuleNode:SetActive(true)
        end
    end
    local modeCahe = SparkPrefs.GetString(Consts.GameType.Billards .. sub_room_mode)
    modeCahe = json.decode(modeCahe)
    if not modeCahe or modeCahe == "" then
        local body = { room_mode = Consts.GameType.Billards, sub_room_mode = sub_room_mode }
        HttpManager.Get(NetCfg.publish_rule, body, true, function(info)
            info = json.decode(info.text)
            showRuleFunc(info)
        end)
    else
        showRuleFunc(modeCahe)
    end
end

function UIBilliardsReady:OnButtonClick(com)
    if com == self.view.roomerModeLabel then
        self.root:OpenAsync(UIBilliardsSelect)
    elseif com == self.view.gatherTipButton then
        self.view.publishRuleNode.gameObject:SetActive(true)
        self:OpenPublishRule()
    elseif com == self.view.closePublishRule or com == self.view.publishBgBtn then
        self.view.publishRuleNode.gameObject:SetActive(false)
    end

    self:OnPrepareButtonClick(com)
end

return UIBilliardsReady