local UIBilliardsPrompt = class("UIBilliardsPrompt", BaseUIBilliardsPrompt)
local Tweening = DG.Tweening
function UIBilliardsPrompt:OnOpened()
    self.showContent = self.context:Get("showContent")
    self.callBack = self.context:Get("callback")
    self.vsSpine = self.view.vsContent.gameObject:GetComponent(typeof(Spine.Unity.SkeletonGraphic))
    self.tipSpine = self.view.tips.gameObject:GetComponent(typeof(Spine.Unity.SkeletonGraphic))
    self.multyHitSpine = self.view.multyHit.gameObject:GetComponent(typeof(Spine.Unity.SkeletonGraphic))

    self:_InitUI()
    self:ShowContent()
end

function UIBilliardsPrompt:_InitUI()

end

function UIBilliardsPrompt:ShowContent()
    self:HideAll()
    local showData = table.remove(self.showContent, 1)
    if showData then
        local showType = showData.showType
        if showType == 1 then   --vs展示
            self:ShowVs()
        elseif showType == 2 then ---消息展示
            self:ShowTips(showData.contentStr)
        elseif showType == 3 then   ---连击展示
            self:ShowMultyHit(showData.player)
        end

        self.timer = self.scheduler:Timeout(function()
            self:ShowContent()
        end, 2.5)
    else
        self.root:Close(UIBilliardsPrompt, true)
        if self.callBack then self.callBack() end
    end
end

function UIBilliardsPrompt:HideAll()
    self.view.vsContent:SetActive(false)
    self.view.multyHit:SetActive(false)
    self.view.tips:SetActive(false)
    self.view.player_1:SetActive(false)
    self.view.player_2:SetActive(false)
end

function UIBilliardsPrompt:ShowVs()
    self.view.vsContent:SetActive(true)
    self.vsSpine.AnimationState:SetAnimation(0, "animation", false)
    local dtSeq = Tweening.DOTween.Sequence()
    dtSeq:AppendInterval(0.2)
    dtSeq:AppendCallback(function()
        self.view.player_1:SetActive(true)
        self.view.player_2:SetActive(true)
    end)

    dtSeq:AppendInterval(0.6)
    dtSeq:AppendCallback(function()
        self.view.player_1:SetActive(false)
        self.view.player_2:SetActive(false)
    end)

    local playerMap = BilliardsBattle:GetPlayers()
    for _, player in pairs(playerMap) do
        local index = player.pos + 1
        local name = self.view["lblPlayerName_"..index]
        local head = self.view["head_"..index]
        name.text = player.nick
        if player.headUrl ~= "" then
            head = head.transform:GetChild(0)
            UIHelper.SetHead(head, player.headUrl, self:GetLoader())
        end
    end
end

function UIBilliardsPrompt:ShowMultyHit(data)
    self.view.multyHit:SetActive(true)
    self.view.multyHit.transform:DOScale(Vector3(1, 1, 1), 0)
    local str = string.format(I18nManager.GetCurrentValue("Billiards_ConEnterTis"), data.con_enter_num)
    self.view.lblMuiltHit.text = I18nManager.ApplyRTLfix(str)
    self.multyHitSpine.AnimationState:SetAnimation(0, "play", false)
    self.view.lblHitPlayerName.text = data.nick
    if data.headUrl ~= "" then
        local head = self.view.multyHitHead.transform:GetChild(0)
        if head then UIHelper.SetHead(head, data.headUrl, self:GetLoader()) end
    end

    local dtSeq = Tweening.DOTween.Sequence()
    dtSeq:AppendInterval(1.5)
    dtSeq:OnComplete(function()
        self:HideAll()
    end)
end

function UIBilliardsPrompt:ShowTips(str)
    self.view.tips:SetActive(true)
    self.view.lblTip.text = str
    self.tipSpine.AnimationState:SetAnimation(0, "play", false)

    local dtSeq = Tweening.DOTween.Sequence()
    dtSeq:AppendInterval(1.5)
    dtSeq:OnComplete(function()
        self:HideAll()
    end)
end

return UIBilliardsPrompt